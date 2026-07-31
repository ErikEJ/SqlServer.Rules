# Investigation: candidate new rules from tsqlrefine (issue #709)

Issue: <https://github.com/ErikEJ/SqlServer.Rules/issues/709>

The issue suggests mining the **tsqlrefine** linter for additional rule ideas:

- Rules reference: <https://github.com/masmgr/tsqlrefine/blob/main/docs/Rules/REFERENCE.md>
- Rule sources: <https://github.com/masmgr/tsqlrefine/tree/main/src/TsqlRefine.Rules/Rules>

This document cross-references the full tsqlrefine catalog (169 rules, captured
from `REFERENCE.md`) against the rules currently published in `docs/readme.md`,
identifies what is already covered, and recommends a prioritized set of
candidates to implement. It follows the same format as
[`issue-439-new-rules.md`](issue-439-new-rules.md).

## How current coverage was mapped

Source-of-truth rule list: `docs/readme.md` (the generated rule inventory).
Verified rule families:

- `SRD####` — Design rules (`src/SqlServer.Rules/Design/`)
- `SRN####` — Naming rules (`src/SqlServer.Rules/Naming/`)
- `SRP####` — Performance rules (`src/SqlServer.Rules/Performance/`)
- `SR####` — built-in Microsoft DacFx rules, surfaced alongside ours

### Two structural caveats about tsqlrefine

1. **tsqlrefine is a text/token linter; SqlServer.Rules is a DacFx *model*
   analyzer.** Several tsqlrefine rules operate on raw file text (blank lines,
   duplicate `GO`, nested block comments, keyword-casing normalization, file
   header conventions). By the time a `SqlCodeAnalysisRule` runs, the input is a
   semantic `TSqlModel` with no notion of the original file layout, so those
   rules are structurally out of scope here.
2. **Many tsqlrefine "semantic/schema" rules re-implement checks that the DacFx
   build already performs.** `unresolved-table-reference`,
   `unresolved-column-reference`, `unresolved-procedure-reference`,
   `index-column-not-in-table`, `insert-column-not-in-table`,
   `group-by-column-mismatch`, `aggregate-in-where-clause`, etc. are all either
   surfaced as model/build errors or rejected by the SQL Server compiler itself.
   They add little as separate rules in a DacFx-based project.

These two facts explain most of the "Skip" verdicts below.

## Coverage matrix

Legend for **Verdict**: **Covered** (an existing rule already handles it),
**Partial** (an existing rule overlaps but with different scope/intent),
**Candidate** (genuinely new, worth implementing), **Skip** (out of scope for a
DacFx model analyzer or too opinionated/noisy).

### Security

| tsqlrefine rule | SqlServer.Rules coverage | Verdict |
|---|---|---|
| avoid-dangerous-procedures (`xp_cmdshell`, `xp_reg*`, `sp_OA*`) | — | **Candidate** (high) |
| avoid-exec-dynamic-sql | `SRD0024` | Covered |
| avoid-execute-as | — | **Candidate** |
| avoid-hardcoded-password | `SRD0075` | Covered |
| avoid-openrowset-opendatasource | — | **Candidate** |
| dynamic-sql-taint | `SRD0096` (SQL injection) | Covered |
| require-parameterized-sp-executesql | `SRD0024` / `SRD0096` | Partial |

### Safety

| tsqlrefine rule | SqlServer.Rules coverage | Verdict |
|---|---|---|
| avoid-merge | — | **Candidate** (opinionated, but KB-backed) |
| cross-database-transaction | — | **Candidate** (low) |
| dangerous-ddl (DROP/TRUNCATE/ALTER..DROP) | — | **Candidate** (mostly for ad-hoc scripts) |
| dml-without-where | `SRD0017` (DELETE), `SRD0018` (UPDATE) | Covered |
| require-drop-if-exists | — | **Candidate** (deployment scripts) |

### Correctness (Critical + Essential + Recommended + Thorough tiers)

| tsqlrefine rule | SqlServer.Rules coverage | Verdict |
|---|---|---|
| require-column-list-for-insert-select / -values | `SRD0015` | Covered |
| require-semicolon-before-throw | — | **Candidate** |
| semantic/duplicate-alias | — | **Candidate** |
| semantic/insert-column-count-mismatch | build/compile error | Skip |
| semantic/undefined-alias | build/compile error | Skip |
| aggregate-in-where-clause | compile error | Skip |
| avoid-ambiguous-datetime-literal (slash dates) | — | **Candidate** |
| avoid-atat-identity | `SRD0056` | Covered |
| avoid-between-for-datetime-range | — | **Candidate** |
| avoid-legacy-join-syntax (`*=`, `=*`) | Microsoft `SR0010` | Covered (DacFx) |
| avoid-named-constraint-in-temp-table | `SRD0092`–`SRD0095` | Covered |
| avoid-not-in-with-null | `SRP0011` (sargability only) | **Candidate** (NULL semantics) |
| avoid-null-comparison | `SRD0011` | Covered |
| avoid-set-rowcount | `SRD0036` | Covered |
| avoid-top-without-order-by-in-select-into | `SRD0014` + `SRD0041` | Partial |
| duplicate-insert-column | — | **Candidate** (runtime error) |
| exec-parameter-count-mismatch | — | **Candidate** (needs catalog) |
| exec-parameter-name-mismatch | — | **Candidate** (needs catalog) |
| group-by-column-mismatch | compile error | Skip |
| having-column-mismatch | compile error | Skip |
| insert-select-column-name-mismatch | — | **Candidate** (info) |
| order-by-in-subquery | `SRD0091` (derived-table ordering) | Partial |
| require-parentheses-for-mixed-and-or | — | **Candidate** |
| semantic/cte-name-conflict | — | **Candidate** |
| semantic/data-type-length | `SRD0026` | Covered |
| semantic/join-condition-always-true (`ON 1=1`) | `SRD0050` / `SRD0076` | Partial |
| semantic/left-join-filtered-by-where | — | **Candidate** (high signal) |
| union-type-mismatch | — | **Candidate** |
| unreachable-case-when (duplicate WHEN) | — | **Candidate** |
| avoid-float-for-decimal | `SRD0046` | Covered |
| avoid-max-plus-one-key-generation | — | **Candidate** |
| avoid-nolock | `SRD0034` | Covered |
| cursor-not-deallocated-on-path | `SRP0007` / `SRP0008` | Covered |
| duplicate-select-column | — | **Candidate** |
| escape-keyword-identifier | Microsoft `SR0012` (partial) | **Candidate** |
| exec-output-not-captured | — | **Candidate** |
| exec-parameter-type-mismatch | — | **Candidate** (needs catalog) |
| semantic/alias-scope-violation | — | **Candidate** (low) |
| semantic/join-table-not-referenced-in-on | `SRD0020` | Partial |
| semantic/return-after-statements (unreachable) | — | **Candidate** |
| semantic/unicode-string (Unicode → VARCHAR) | — | **Candidate** |
| string-agg-nvarchar-max | — | **Candidate** (niche) |
| string-agg-without-order-by | — | **Candidate** |
| string-assignment-length-mismatch | — | **Candidate** |
| stuff-without-order-by | — | **Candidate** |
| unreachable-statement | — | **Candidate** |
| variable-used-before-assignment | — | **Candidate** |
| circular-object-reference | — | **Candidate** (model-level) |
| inconsistent-result-set | — | **Candidate** (complex) |
| len-for-emptiness-check (LEN vs DATALENGTH) | — | **Candidate** |
| mixed-string-length-functions-in-loop | — | Skip (very niche) |
| multi-row-update-from | — | **Candidate** (see also update-join-cardinality) |
| semantic/set-variable (prefer SELECT) | — | Skip (opinionated / debatable) |
| unreferenced-object | — | **Candidate** (model-level, low) |
| unused-variable | `SRD0012` | Covered |
| unresolved-procedure-reference | build error | Skip |

### Performance

| tsqlrefine rule | SqlServer.Rules coverage | Verdict |
|---|---|---|
| top-without-order-by | `SRD0014` | Covered |
| avoid-correlated-subquery-in-select | `SRP0024` | Partial |
| avoid-cursors | `SRD0033` | Covered |
| avoid-implicit-conversion-in-predicate | `SRP0027` | Covered |
| avoid-non-sargable-predicate | `SRP0009` / `SRP0015` | Covered |
| avoid-optional-parameter-pattern (`@p IS NULL OR col=@p`) | — | **Candidate** (high) |
| avoid-query-hints | `SRD0030` | Covered |
| avoid-scalar-udf-in-query | `SRP0010` (DML only) | **Candidate** (SELECT scope) |
| avoid-select-star | `SRD0006` | Covered |
| avoid-top-100-percent-order-by | `SRD0081` | Covered |
| avoid-top-in-dml | — | **Candidate** |
| like-leading-wildcard | `SRP0002` | Covered |
| prefer-exists-over-in-subquery | `SRD0021` | Covered |
| redundant-semi-join | — | **Candidate** (low) |
| avoid-full-text-search | — | Skip (opinionated) |
| avoid-information-schema | — | **Candidate** (low/info) |
| avoid-linked-server | `SRP0026` | Covered |
| avoid-objectproperty | — | **Candidate** (low) |
| avoid-or-on-different-columns | `SRD0032` | Covered |
| avoid-select-distinct | `SRP0003` (aggregate only) | **Candidate** (broaden) |
| avoid-select-into | `SRD0041` | Covered |
| avoid-upper-lower-in-predicate | `SRP0009` (functions on column) | Partial |
| deep-view-nesting | `SRP0001` (nested views) | Partial |
| max-cyclomatic-complexity | — | **Candidate** (metric) |
| max-joins-per-query | `SRP0018` | Covered |
| max-nesting-depth | — | **Candidate** (metric) |
| max-parameter-count | — | **Candidate** (metric) |
| max-statement-count | — | **Candidate** (metric) |
| prefer-utc-datetime | — | Skip (opinionated) |
| require-data-compression | — | Skip (opinionated) |

### Transactions

| tsqlrefine rule | SqlServer.Rules coverage | Verdict |
|---|---|---|
| avoid-transaction-without-commit | `SRD0009` (partial intent) | Partial |
| transaction-not-closed-on-path | `SRD0009` (partial intent) | Partial |
| uncommitted-transaction | `SRD0009` | Partial |
| avoid-catch-swallowing | — | **Candidate** |
| require-save-transaction-in-nested | — | **Candidate** (low) |
| require-throw-or-raiserror-in-catch | — | **Candidate** |
| require-try-catch-for-transaction | `SRD0013` | Partial |
| set-ansi (ANSI_NULLS ON) | `SRD0085` | Covered |
| set-ansi-padding | `SRD0086` | Covered |
| set-ansi-warnings | `SRD0087` | Covered |
| set-arithabort | — | **Candidate** (low) |
| set-concat-null-yields-null | `SRD0084` | Covered |
| set-nocount | `SRP0005` | Covered |
| set-quoted-identifier | `SRD0089` | Covered |
| set-transaction-isolation-level | — | Skip (context-dependent) |
| set-xact-abort | `SRD0069` | Covered |

### Schema

| tsqlrefine rule | SqlServer.Rules coverage | Verdict |
|---|---|---|
| avoid-deprecated-types (TEXT/NTEXT/IMAGE/TIMESTAMP) | `SRD0051` | Covered |
| delete/insert/update-column-not-in-table | build/compile error | Skip |
| duplicate-column-definition | compile error | Skip |
| duplicate-table-function-column | compile error | Skip |
| duplicate-table-variable-column | compile error | Skip |
| duplicate-view-column | compile error | Skip |
| duplicate-foreign-key-column | — | **Candidate** |
| duplicate-index-column | — | **Candidate** |
| duplicate-index-definition | `SRD0052` | Covered |
| implicit-conversion-in-predicate-schema | `SRP0016` / `SRP0027` | Covered |
| index-column-not-in-table | build error | Skip |
| join-column-deviation | — | Skip (needs relation profile) |
| join-foreign-key-mismatch | `SRD0020` | Partial |
| unresolved-column-reference | build error | Skip |
| unresolved-table-reference | build error | Skip |
| update-join-cardinality-mismatch | — | **Candidate** |
| avoid-heap-table | `SRP0020` | Covered |
| require-primary-key-or-unique-constraint | `SRD0002` | Covered |
| require-table-description | — | Skip (opinionated) |

### Style

| tsqlrefine rule | SqlServer.Rules coverage | Verdict |
|---|---|---|
| avoid-order-by-ordinal | `SRD0025` | Covered |
| prefer-unicode-string-literals (`N'...'`) | — | **Candidate** (opinionated) |
| require-qualified-columns-everywhere | `SRD0028` | Partial |
| require-schema-qualify-exec | `SRD0039` | Covered |
| semantic/multi-table-alias | `SRD0028` | Partial |
| semantic/schema-qualify | `SRD0039` / `SRN0006` | Covered |
| normalize-inequality-operator (`!=` → `<>`) | — | **Candidate** (fixable style) |
| prefer-concat-with-nullable | — | **Candidate** |
| qualified-select-columns | `SRD0028` | Partial |
| semantic/case-sensitive-variables | — | **Candidate** (low) |
| avoid-magic-convert-style-for-datetime | — | **Candidate** (low) |
| duplicate-empty-line | — | Skip (text-level) |
| duplicate-go | — | Skip (text-level) |
| nested-block-comments | — | Skip (text-level) |
| normalize-execute-keyword (`EXEC` → `EXECUTE`) | — | Skip (cosmetic) |
| normalize-procedure-keyword (`PROC` → `PROCEDURE`) | — | Skip (cosmetic) |
| normalize-transaction-keyword (`TRAN` → `TRANSACTION`) | — | Skip (cosmetic) |
| prefer-coalesce-over-nested-isnull | — | **Candidate** |
| prefer-concat-over-plus | — | **Candidate** |
| prefer-concat-ws | — | **Candidate** (low) |
| prefer-json-functions | — | Skip (opinionated) |
| prefer-string-agg-over-stuff | — | **Candidate** (low) |
| prefer-trim-over-ltrim-rtrim | — | **Candidate** (fixable) |
| prefer-try-convert-patterns | — | **Candidate** (low) |
| require-as-for-column-alias | — | **Candidate** (style) |
| require-as-for-table-alias | — | **Candidate** (style) |
| require-begin-end-for-while | `SRD0066` (conditionals only) | **Candidate** (extend to WHILE) |
| require-begin-end-lenient / -strict | `SRD0066` | Covered |
| require-explicit-join (comma joins) | `SRD0020` (predicate only) | **Candidate** |
| require-explicit-join-type | — | **Candidate** |
| semicolon-termination | `SRD0068` | Covered |

### Debug

| tsqlrefine rule | SqlServer.Rules coverage | Verdict |
|---|---|---|
| avoid-print-statement | — | **Candidate** (low/opinionated) |

## Recommended candidates, ranked

Ranking weighs: (a) signal-to-noise (how often the warning is genuinely
actionable), (b) implementation cost in ScriptDom, (c) whether the smell is
specific to T-SQL, and (d) whether it is already implicitly caught by the DacFx
build.

### Tier 1 — implement next (high value, cheap, low false-positive rate)

1. **Dangerous extended stored procedures** (`avoid-dangerous-procedures`)
   - Flag calls to `xp_cmdshell`, `xp_regread`/`xp_reg*`, `sp_OACreate`/`sp_OA*`,
     `xp_dirtree`, etc. Pure security value, near-zero false positives.
   - Implementation: `ExecutableProcedureReference` / `ExecuteStatement` name
     match against a curated denylist. Category **Design** (security) — not
     ignorable.
   - Suggested ID: `SRD0097`.

2. **`OPENROWSET` / `OPENDATASOURCE`** (`avoid-openrowset-opendatasource`)
   - Ad-hoc remote data access; well-known attack/exfiltration surface.
   - Implementation: `OpenRowsetTableReference` / `OpenQueryTableReference` node
     visit. Security category, ignorable.
   - Suggested ID: `SRD0098`.

3. **`EXECUTE AS` privilege escalation** (`avoid-execute-as`)
   - Detect `EXECUTE AS` clauses in module definitions / standalone statements.
   - Implementation: `ExecuteAsClause` / `ExecuteAsStatement`. Ignorable.
   - Suggested ID: `SRD0099`.

4. **Duplicate column in `INSERT` column list** (`duplicate-insert-column`)
   - Always a runtime error; high-confidence, trivial. Sibling to the DacFx
     duplicate-column-in-table check but for the DML column list.
   - Implementation: walk `InsertStatement.InsertSpecification.Columns`,
     case-insensitive duplicate detection using the shared `Comparer`.
   - Suggested ID: `SRD0100` — not ignorable.

5. **LEFT JOIN filtered by WHERE** (`semantic/left-join-filtered-by-where`)
   - A `LEFT JOIN` whose right-side column appears in a non-`IS NULL` `WHERE`
     predicate silently becomes an `INNER JOIN` — a classic real bug.
   - Implementation: correlate `OuterJoin` right-table aliases against
     column references in the `WhereClause` (excluding `IS NULL`). Ignorable.
   - Suggested ID: `SRP0031` (Performance/Correctness).

6. **`ORDER BY` without explicit sort direction / `NEWID()`/`RAND()` ordering**
   - Already recommended in `issue-439-new-rules.md`; tsqlrefine corroborates
     via `avoid-order-by-ordinal` neighbours. Keep those Tier-1 items from #439.

### Tier 2 — worth implementing, moderate scope

7. **Optional-parameter "kitchen sink" pattern**
   (`avoid-optional-parameter-pattern`)
   - `@p IS NULL OR col = @p` and `col = ISNULL(@p, col)` cause parameter-sniffing
     plan instability. High value for procedure-heavy codebases.
   - Implementation: pattern-match `BooleanBinaryExpression` (OR) with one side a
     `@p IS NULL` and the other `col = @p`. Ignorable.
   - Suggested ID: `SRP0032`.

8. **Scalar UDF used in a query's SELECT/WHERE** (`avoid-scalar-udf-in-query`)
   - Extend beyond `SRP0010` (which only covers UDFs in DML). Row-by-row scalar
     UDFs are a major performance foot-gun in `SELECT`/`WHERE`.
   - Requires resolving whether a `FunctionCall` targets a user scalar function
     via the model catalog. Ignorable.
   - Suggested ID: `SRP0033`.

9. **`UNION` where `UNION ALL` suffices** — carried over from
   `issue-439-new-rules.md` (sonar C015 / sqlcheck 3015); tsqlrefine has no
   direct equivalent but the smell is the same. Ignorable.

10. **BETWEEN for datetime ranges** (`avoid-between-for-datetime-range`)
    - `BETWEEN @start AND @end` on datetime silently drops sub-day precision at
      the upper bound. Detect `BETWEEN` where an operand type resolves to a
      date/time type via `GetDataType`. Ignorable.
    - Suggested ID: `SRP0034`.

11. **Slash-delimited / ambiguous date literals**
    (`avoid-ambiguous-datetime-literal`)
    - Flag string date literals like `'01/02/2024'` assigned/compared to a
      date/time column — locale-dependent. Ignorable.
    - Suggested ID: `SRD0101`.

12. **Missing parentheses in mixed AND/OR** (`require-parentheses-for-mixed-and-or`)
    - Detect a `BooleanBinaryExpression` mixing `AND` and `OR` without explicit
      grouping. Ignorable.
    - Suggested ID: `SRD0102`.

13. **CATCH-block hygiene** (`avoid-catch-swallowing` /
    `require-throw-or-raiserror-in-catch`)
    - A `CATCH` block that neither re-throws (`THROW`/`RAISERROR`) nor logs is a
      silent-failure trap. One rule, ignorable.
    - Suggested ID: `SRD0103`.

14. **Duplicate columns in an index / FK / PK definition**
    (`duplicate-index-column`, `duplicate-foreign-key-column`)
    - Walk `IndexDefinition`/`Constraint` column lists for duplicates. Sibling to
      `SRD0052`. Ignorable.
    - Suggested ID: `SRD0104`.

### Tier 3 — style / opinionated, ship as opt-in (ignorable, default surfaced but easy to disable)

These are legitimate but taste-dependent. Each is a small ScriptDom check.

- **`prefer-trim-over-ltrim-rtrim`** — `LTRIM(RTRIM(x))` → `TRIM(x)` (2017+).
- **`prefer-coalesce-over-nested-isnull`** — nested `ISNULL` → `COALESCE`.
- **`normalize-inequality-operator`** — `!=` → `<>` (already have `SRP0006`
  which discourages inequality entirely; this is the milder style variant).
- **`require-as-for-column-alias` / `require-as-for-table-alias`** — require the
  `AS` keyword on aliases.
- **`require-explicit-join` / `require-explicit-join-type`** — comma joins and
  bare `JOIN` shorthand (overlaps the `issue-439` C023 cartesian-join candidate).
- **`require-begin-end-for-while`** — extend `SRD0066` to `WHILE` bodies.
- **`avoid-print-statement`** — discourage `PRINT` in modules.
- **`len-for-emptiness-check`** — `LEN(x) = 0` → `DATALENGTH(x) = 0` for
  whitespace-only detection.

### Metric-style rules (nice-to-have, need a threshold config story)

`max-cyclomatic-complexity`, `max-nesting-depth`, `max-parameter-count`,
`max-statement-count`. SqlServer.Rules already ships threshold rules (`SRP0018`
high join count, `SRD0045` excessive indexes), so these fit the existing style —
but a shared, configurable threshold mechanism would be worth designing first
rather than hard-coding limits per rule.

### Rules that need an authoritative object catalog

`exec-parameter-count-mismatch`, `exec-parameter-name-mismatch`,
`exec-parameter-type-mismatch`, `exec-output-not-captured`. These are genuinely
valuable and *not* redundant with the DacFx build (which does not validate
`EXEC` argument binding), but they require resolving the callee's parameter
signature from the model. Worthwhile as a small focused group once one of them
establishes the signature-lookup helper.

## Explicit non-candidates

Documented so they are not re-proposed:

- **Text-layout rules** (`duplicate-empty-line`, `duplicate-go`,
  `nested-block-comments`, keyword-casing `normalize-*`, file-header checks).
  SqlServer.Rules runs on a semantic model, not on source text; there is no file
  layout to inspect at analysis time. (Keyword casing is partially addressed by
  the CLI's `-f` formatter, not by a rule.)
- **Compile-time errors re-surfaced as lint** (`aggregate-in-where-clause`,
  `group-by-column-mismatch`, `having-column-mismatch`,
  `duplicate-column-definition`, `*-column-not-in-table`,
  `insert-column-count-mismatch`, `unresolved-*-reference`). The DacFx build /
  SQL Server compiler already rejects these; a separate rule adds nothing in a
  project-based workflow.
- **`avoid-select-distinct` (blanket)** — `DISTINCT` is frequently legitimate;
  only the aggregate variant (`SRP0003`) has an acceptable signal-to-noise ratio.
  A broadened version would be very noisy.
- **`prefer-utc-datetime`, `require-data-compression`, `require-table-description`,
  `prefer-json-functions`, `avoid-full-text-search`, `set-transaction-isolation-level`**
  — each imposes an architectural preference that is wrong for many valid
  codebases. Skip, or only as strictly opt-in if ever requested.
- **`semantic/set-variable` (prefer `SELECT` over `SET`)** — contradicts common
  guidance (`SET` is the safer single-value assignment); do not adopt.
- **`join-column-deviation`, `join-foreign-key-mismatch` (profile-based)** —
  depend on a "dominant relation profile" tsqlrefine builds across a corpus;
  `SRD0020` already covers the actionable FK/predicate cases.

## Implementation notes for whichever rules are picked up

- New design/perf rules: follow the existing template (`Design/AliasTablesRule.cs`
  is a clean reference). The const triple is `RuleId` / `RuleDisplayName` /
  `Message`; the `[ExportCodeAnalysisRule]` attribute carries `Category` from
  `Globals/Constants.cs` and typically `RuleScope = SqlRuleScope.Element`.
- Prefer reusing an existing visitor under `src/SqlServer.Rules/Visitors/`
  (~90 exist) rather than writing a new AST walk.
- Type-aware rules (BETWEEN-on-datetime, Unicode→VARCHAR, scalar-UDF) should use
  the cached `GetDataType` / `GetColumnDataType` helpers on
  `BaseSqlCodeAnalysisRule` — do not bypass the `ConditionalWeakTable` caches.
- Mark each rule **Ignorable** (`<IsIgnorable>true</IsIgnorable>` in the class
  XML doc) unless the violation is universally a defect (e.g. dangerous procs,
  duplicate insert column).
- Add SQL fixtures under `sqlprojects/TSQLSmellsTest/` and a corresponding
  `SR####Tests.cs` under `test/SqlServer.Rules.Test/<Category>/`. Smoke-test
  fixtures (AW, Chinook, Fabric) will likely need their `ExpectedProblems` lists
  updated when a broadly-applicable rule lands.
- Do **not** edit anything under `docs/` directly — re-run
  `SqlServer.Rules.DocsGenerator` after implementation so the published rule list
  regenerates.

## Suggested next step

Open one issue per Tier-1 rule (or a single tracking issue with checkboxes) so
they can be implemented independently. Tier-1 rules 1–5 are each ~40–70 lines of
rule code plus a fixture; the catalog-dependent `EXEC` argument-binding family
and the metric/threshold rules each warrant a small design note first.
