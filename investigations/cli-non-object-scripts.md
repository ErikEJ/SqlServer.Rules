# Proposal: Analyzing non–object-creation scripts in the CLI

Status: **implemented** (Option A)
Scope: `tools/SqlAnalyzerCli` + `tools/ErikEJ.DacFX.TSQLAnalyzer`

## Problem

`tsqlanalyze` (and the underlying `ErikEJ.DacFX.TSQLAnalyzer` engine) can only
analyze **object-creation scripts** — files whose batches are `CREATE TABLE`,
`CREATE PROCEDURE`, `CREATE VIEW`, `CREATE FUNCTION`, etc.

Scripts that are *not* object definitions cannot be analyzed today:

- ad-hoc query / investigation scripts (`SELECT * FROM ...`, `UPDATE ...`)
- migration / deployment / post-deploy scripts (`INSERT`, `MERGE`, `EXEC`, `GO`-separated batches)
- maintenance scripts (`ALTER`, index rebuilds, `DBCC`, dynamic SQL)

For these inputs the tool either reports a model error or silently finds zero
problems — even though many design/performance rules (`SELECT *`, `NOLOCK`,
positional `ORDER BY`, non-SARGable predicates, etc.) apply just as well inside
an ad-hoc batch as inside a stored procedure body.

## Root cause

The engine is **model-based**. DacFx code analysis runs against a `TSqlModel`,
and rules execute against the *elements* in that model (`SqlRuleScope.Element`)
or the model as a whole (`SqlRuleScope.Model`).

The model is populated exclusively from object definitions:

| Where | Call |
|---|---|
| `AnalyzerFactory.AddFilesToModel` | `model.AddOrUpdateObjects(fileContents, fileName, …)` (`AnalyzerFactory.cs:224`) |
| `AnalyzerFactory.AddScriptToModel` | `model.AddObjects(script, …)` (`AnalyzerFactory.cs:238`) |

`AddObjects` / `AddOrUpdateObjects` only materialize **schema objects**. A batch
such as `SELECT * FROM dbo.Foo;` contributes no element to the model, so there
is nothing for an element-scoped rule to visit. The MCP tool makes this contract
explicit and rejects anything else:

```csharp
// AnalyzerTools.cs:22
if (!sqlScript.Trim().StartsWith("CREATE ", StringComparison.OrdinalIgnoreCase))
{
    return "The script must be an object creation script starting with 'CREATE'";
}
```

So the limitation is architectural, not a missing CLI flag.

## Options considered

### Option A — Auto-wrap ad-hoc batches in a synthetic stored procedure (recommended)

Before adding a batch to the model, detect whether its top-level statement is an
object definition. If it is, add it as-is (current behavior). If it is **not**,
wrap the batch body in a generated, throw-away procedure so it becomes a model
element:

```sql
CREATE PROCEDURE [__tsqlanalyzer].[adhoc_<n>]
AS
BEGIN
    <original batch text>
END
```

The procedure body is exactly the kind of element the existing rules already
analyze, so the bulk of the design/performance ruleset starts firing against
ad-hoc T-SQL **with no rule changes**.

Pros:
- Reuses the entire existing engine and ruleset.
- Smallest, lowest-risk change; isolated to the model-building path.
- Matches the technique the rules were designed for (procedure bodies).

Cons / things to handle:
- **Line-number remapping.** The wrapper prefixes lines; every reported
  `StartLine` for a wrapped batch must be shifted back by the number of prefix
  lines (and offset by the batch's position within the file). Reported
  `SourceName` must map back to the real file, not the synthetic object.
- **Not every statement is legal inside a procedure body.** Batch-only
  constructs (`USE`, `SET SHOWPLAN_*`, `CREATE/ALTER` of certain objects,
  `BACKUP`, etc.) fail when nested. These batches should be skipped (and
  optionally surfaced as "not analyzable") rather than aborting the whole file.
- **Batch separation.** Files are `GO`-delimited. Wrap each batch independently
  so one un-wrappable batch doesn't lose the rest.
- Rules whose semantics are "an object named X should…" (naming rules,
  cross-object model rules) are meaningless for the synthetic object and should
  be excluded from wrapped batches (see *Rule applicability* below).

### Option B — ScriptDom-only (model-less) analysis path

Run a curated subset of rules directly over the ScriptDom AST, bypassing the
DacFx code-analysis service entirely.

Pros: no synthetic objects, no line remapping.
Cons: the rules in this repo inherit from `BaseSqlCodeAnalysisRule` /
`SqlCodeAnalysisRule` and rely on model context (e.g. `GetDataType` /
`GetColumnDataType` resolve column types from the model). Only rules that are
purely syntactic could run; everything that needs type/column resolution would
have to be re-implemented or skipped. This is a large rework for partial
coverage and is **not recommended** as the first step.

### Option C — Document the limitation only

Make the constraint explicit in `readme.md` and the MCP error message and stop
there. Lowest effort, no new capability. Acceptable as a stop-gap but does not
solve the request.

## Recommendation

Implement **Option A**. It unlocks the most rules for the least risk and keeps
a single analysis engine.

## Rule applicability (Option A)

Three buckets once a batch is wrapped:

| Bucket | Behavior |
|---|---|
| Design / performance rules that inspect statement bodies (`SELECT *`, `NOLOCK`, positional `ORDER BY`, `EXEC` dynamic SQL, non-SARGable predicates, cursors, `WAITFOR DELAY`, …) | **Fire normally** — primary value of this feature. |
| Naming rules (`SRN*`) that judge the object's own name/shape | **Suppress** for synthetic wrappers — the generated name is not the user's. |
| Whole-model / cross-object rules (`SqlRuleScope.Model`, e.g. missing-PK, FK-index) | **Suppress / best-effort** — no real schema is present for ad-hoc input. |

Suppression can reuse the existing problem-suppressor mechanism already wired in
`AnalyzerFactory.Analyze` (`service.SetProblemSuppressor(...)`, `AnalyzerFactory.cs:63`)
combined with the synthetic schema/object name.

## Proposed CLI surface

Two sub-options; **A1 is preferred**.

- **A1 — automatic.** No new switch. When a batch is not an object definition,
  the engine wraps it transparently. "Just works" for the SSMS external-tool and
  ad-hoc-file scenarios. Add a one-line note to output that *N* batches were
  analyzed as ad-hoc.

- **A2 — opt-in switch.** Add `--adhoc` (`-A`) to `CliAnalyzerOptions` /
  `AnalyzerOptions`, off by default, to preserve today's strict behavior unless
  requested:

  ```bash
  ## Analyze an ad-hoc / migration script (no CREATE wrapper)
  tsqlanalyze -i C:\scripts\migration_042.sql --adhoc
  ```

A pragmatic combination: default to A1 for `.sql` *files/folders* and the `-t`
text input, but keep the MCP `FindSqlScriptProblems` contract opt-in so existing
callers are unaffected.

## Implementation sketch

All changes are confined to the engine's model-building path plus options:

1. **`ErikEJ.DacFX.TSQLAnalyzer/Services`** — add a `BatchWrapper` helper:
   - Parse input with ScriptDom (`TSqlParser` for the requested `SqlVersion`)
     into `GO`-separated batches.
   - For each batch, inspect the top-level `TSqlStatement`: if it is a
     `CreateXxxStatement` → keep verbatim; else wrap in the synthetic procedure
     and record `(realFile, lineOffset)`.
   - Skip batches that are illegal inside a procedure body; collect them as
     "not analyzable" diagnostics.

2. **`AnalyzerFactory.AddFilesToModel` / `AddScriptToModel`** — route content
   through `BatchWrapper` before `AddObjects` / `AddOrUpdateObjects`
   (`AnalyzerFactory.cs:210`, `:233`). Keep a map from synthetic object →
   `(originalFile, lineOffset)`.

3. **Result post-processing** — when emitting problems
   (`AnalyzerFactory.SaveOutputFile` and the console loop in `Program.Run`,
   `Program.cs:205`), translate synthetic `SourceName`/`StartLine` back to the
   real file and line using the map, and apply the rule-bucket suppression.

4. **Options** — add `Adhoc` to `AnalyzerOptions` and a matching
   `-A/--adhoc` option in `CliAnalyzerOptions` if A2 is chosen; otherwise wire
   A1 unconditionally for file/text input.

5. **MCP** — relax `AnalyzerTools.FindSqlScriptProblems` (`AnalyzerTools.cs:22`):
   keep accepting `CREATE` scripts, but when `--adhoc`/wrapping is enabled,
   analyze non-`CREATE` scripts too and adjust the description text.

6. **Docs / help** — add an "Analyze an ad-hoc script" sample to
   `Program.DisplayHelp` (`Program.cs:277`) and to `readme.md`, and note the
   line-remapping + suppressed-rule caveats.

## Tests

- New fixtures under `sqlprojects/` containing ad-hoc batches (DML, `GO`-separated
  multi-batch, one un-wrappable batch mixed with analyzable ones).
- Engine tests in `test/TSQLAnalyzer.Tests` asserting:
  - design/perf rules fire on wrapped batches,
  - reported line numbers map back to the **original** file lines,
  - naming / model-scope rules are suppressed for synthetic objects,
  - un-wrappable batches are reported as not-analyzable, not fatal.

## Implementation (as built)

Option A was implemented as an **always-on, transparent** behavior (sub-option
A1). No new CLI switch was added; library consumers can opt out via
`AnalyzerOptions.WrapAdhocBatches` (default `true`). The MCP tool was changed to
follow the same path.

- **`Services/BatchWrapper.cs`** (new) — parses the script with `TSql170Parser`
  and, for each `GO`-separated batch whose statements are all in an allow-list of
  body-legal DML / control-flow statements, splices
  `CREATE PROCEDURE [dbo].[__tsqlanalyzer_adhoc_batch_N] AS BEGIN … END` around
  the batch. The prefix/suffix are inserted **inline (no new lines)**, so the
  approach needs *no* line-offset map — every reported `StartLine` already
  matches the real source line. (Only the column on a wrapped batch's first line
  shifts.) Object definitions and batches containing batch-only statements are
  left untouched; unparseable input is returned verbatim (unchanged behavior).
- **`AnalyzerFactory`** — `AddFilesToModel` / `AddScriptToModel` route content
  through `BatchWrapper.Wrap` before `AddOrUpdateObjects` / `AddObjects`. The
  problem suppressor now also drops naming rules (`SRN*`, `SR0011/0012/0016`) and
  procedure-shape rules (`SRP0005` SET NOCOUNT ON) when they are raised against a
  synthetic `__tsqlanalyzer_adhoc_batch_*` object, keyed off
  `SqlRuleProblemSuppressionContext.ModelElement`.
- **`AnalyzerTools.FindSqlScriptProblems`** (MCP) — the `CREATE`-prefix gate was
  removed; ad-hoc batches are analyzed too.

Verified: the rule + smoke test suites (106 tests) and the analyzer-engine tests
remain green (DDL-only fixtures are unaffected); a multi-batch DML script now
reports `SELECT *`, single-char-alias, `IN`-subquery and semicolon problems at
their real line numbers, with no synthetic-wrapper noise.

Tests (added): `test/TSQLAnalyzer.Tests/BatchWrapperTests.cs` covers `BatchWrapper.Wrap`
directly (object definitions left unchanged, DML batches wrapped, inline wrapping
preserves line count, each `GO`-separated batch wrapped independently, object
definitions and batch-only statements left untouched, unparseable input returned
verbatim). `test/TSQLAnalyzer.Tests/AdhocAnalysisTests.cs` covers the engine end to
end against new fixtures under `sqlprojects/AdhocScripts/` (design rules fire on
wrapped batches, reported line numbers map back to the original source line,
naming / `SRP0005` / synthetic-object noise is suppressed, every `GO`-separated
batch is analyzed, and a mixed real-object + ad-hoc file analyzes without fatal
model errors). `BatchWrapper` is exposed to the test project via `InternalsVisibleTo`.

Not yet done: CLI `--help` sample and `readme.md` note.

## Open questions

- Should the synthetic-object schema (`__tsqlanalyzer`) be configurable to avoid
  collisions if a user's ad-hoc script references such a name? (Use a GUID-ish
  suffix to be safe.)
- For multi-batch files that mix real `CREATE` objects and ad-hoc batches, do we
  analyze both in the same model run, or two passes? (Single pass is feasible and
  preferred.)
- How to present per-batch "not analyzable" notices without adding noise to the
  problem list (separate summary line vs. dedicated diagnostic).
