# Investigation: candidate new rules (issue #439)

Issue: <https://github.com/ErikEJ/SqlServer.Rules/issues/439>

The issue suggests mining two third-party SQL linters for additional rule ideas:

1. **sonar-sql-plugin** — <https://github.com/gretard/sonar-sql-plugin/blob/master/docs/pluginRules.md>
2. **sqlcheck** — <https://github.com/jarulraj/sqlcheck/tree/master/docs>

This document cross-references both rule lists against the rules currently published in `docs/readme.md`, identifies what is already covered, and recommends concrete candidates to implement.

## How current coverage was mapped

Source-of-truth rule list: `docs/readme.md` (the generated rule inventory). Verified rule families:

- `SRD####` — Design rules (`src/SqlServer.Rules/Design/`)
- `SRN####` — Naming rules (`src/SqlServer.Rules/Naming/`)
- `SRP####` — Performance rules (`src/SqlServer.Rules/Performance/`)
- `SR####` — built-in Microsoft DacFx rules, surfaced alongside ours

## sonar-sql-plugin (gretard) — coverage

| sonar id | Title | SqlServer.Rules coverage | Verdict |
|---|---|---|---|
| C001 | SLEEP/WAITFOR | `SRD0035` (Forced delay) | Covered |
| C002 | `SELECT *` | `SRD0006` + Microsoft `SR0001` | Covered |
| C003 | `INSERT` without column list | `SRD0015` | Covered |
| C004 | Positional `ORDER BY` | `SRD0025` | Covered |
| C005 | `EXEC`/`EXECUTE` for dynamic SQL | `SRD0024` | Covered |
| C007 | `NOLOCK` hint | `SRD0034` | Covered |
| C009 | Non-sargable predicate | `SRP0002/0006/0009/0011/0015/0016/0027` family | Covered |
| C010 | PK naming `PK_` | `SRN0007` is generic only | **Candidate** |
| C011 | FK naming `FK_` | `SRN0007` is generic only | **Candidate** |
| C012 | `= NULL` / `<> NULL` | `SRD0011` | Covered |
| C013 | Index naming `IX_` | `SRN0007` is generic only | **Candidate** |
| C014 | `OR` in `WHERE` | `SRD0032` | Covered |
| C015 | `UNION` should be `UNION ALL` | — | **Candidate** |
| C016 | `IN`/`NOT IN` with subquery | `SRD0021`, `SRP0011` | Covered |
| C017 | `ORDER BY` without explicit `ASC`/`DESC` | — | **Candidate** |
| C020 | Hints | `SRD0030` | Covered |
| C021 | Missing `COMMIT` after DML | `SRD0009` only flags missing-transaction-wrap | Partial — different intent; **low priority** |
| C022 | Non-materialized view used | — | **Skip** — too opinionated for the dialect; indexed views require many preconditions |
| C023 | Cartesian join (`FROM a, b`) | `SRD0020` flags missing/incomplete join predicates only | **Candidate** — flag the comma-join syntax explicitly |
| C030 | Missing file header comment | — | **Skip** — DacFx model has no file concept by the time rules run |

## sqlcheck (jarulraj) — coverage

| sqlcheck id | Title | SqlServer.Rules coverage | Verdict |
|---|---|---|---|
| 1001 | Multi-valued attribute (CSV in column) | — | **Skip** — not statically detectable |
| 1002 | Recursive relationship via FK | — | **Skip** — design-level, low signal |
| 1003 | Missing primary key | `SRD0002` | Covered |
| 1004 | Generic PK name (`id`) | — | **Candidate (opinionated)** — easy check, but many teams accept `Id` |
| 1005 | Missing foreign key | `SRD0020` flags missing-FK on joins; not table-level FK absence | Partial — current rule is query-shape only |
| 1006 | Entity-Attribute-Value | — | **Skip** — heuristic, very noisy |
| 1007 | Metadata tribbles (table-per-year) | — | **Skip** — design heuristic |
| 2001 | Imprecise type (`FLOAT`/`REAL`) | `SRD0046` | Covered |
| 2002 | Values in column definition (enum strings) | — | **Skip** — `CHECK ... IN (...)` is idiomatic in T-SQL |
| 2003 | Files as data types | — | **Skip** — FILESTREAM is a legitimate choice |
| 2004 | Too many indexes | `SRD0045` | Covered |
| 2005 | Index attribute order | — | **Skip** — not statically inferable without workload data |
| 3001 | `SELECT *` | `SRD0006` | Covered |
| 3002 | `NULL` usage | `SRD0011` | Partial |
| 3003 | `NOT NULL` usage everywhere | — | **Skip** — backwards: warning about good practice |
| 3004 | String concat with potentially-NULL columns | — | **Candidate** — relevant when `CONCAT_NULL_YIELDS_NULL` is ON (which `SRD0084` already requires) |
| 3005 | `GROUP BY` non-grouped columns | — | **Skip** — SQL Server rejects this at compile time |
| 3006 | `ORDER BY RAND()` / `NEWID()` | — | **Candidate** |
| 3007 | Pattern matching (`LIKE`) | `SRP0002` | Covered |
| 3008 | Spaghetti query | — | **Skip** — too subjective |
| 3009 | Reduce JOIN count | `SRP0018` | Covered |
| 3010 | Unnecessary `DISTINCT` | `SRP0003` covers DISTINCT-in-aggregate only | **Candidate (narrow scope)** |
| 3011 | Implicit column list | `SRD0015` | Covered |
| 3012 | `HAVING` without aggregate (could be `WHERE`) | — | **Candidate** |
| 3013 | Nested subqueries | — | **Skip** — modern optimizer handles fine |
| 3014 | `OR` → `IN` | `SRD0032` | Covered |
| 3015 | `UNION` → `UNION ALL` | — | **Candidate** (same as sonar C015) |
| 3016 | `DISTINCT` + `JOIN` | — | **Candidate (low priority)** |
| 4001 | Cleartext passwords | `SRD0075` | Covered |

## Recommended candidates, ranked

Ranking weighs: (a) signal-to-noise (how often the warning is genuinely actionable), (b) implementation cost in ScriptDom, (c) whether the smell is specific to T-SQL.

### Tier 1 — implement next

1. **Convention prefix rules for keys/indexes** (sonar C010/C011/C013)
   - Two new rules (or a single configurable rule) checking that constraint and index names start with conventional prefixes: `PK_`, `FK_`, `UQ_`, `CK_`, `IX_`, `DF_`.
   - Implementation: walk `CreateTableStatement.Definition.TableConstraints` for inline constraints and `CreateIndexStatement` / `CreateTableStatement` for index names. Existing `Naming/` rules already inspect identifiers (e.g. `SRN0007`), and `Visitors/CreateIndexStatementVisitor.cs` already exists. Should be ignorable.
   - Risk: prefix conventions vary by shop. Ship as separate ignorable rules (one per object kind) so teams can enable selectively.
   - Suggested IDs: `SRN0010` (PK), `SRN0011` (FK), `SRN0012` (UQ), `SRN0013` (CK), `SRN0014` (IX), `SRN0015` (DF) — or one configurable rule.

2. **`UNION` where `UNION ALL` would do** (sonar C015 / sqlcheck 3015)
   - Pattern: `QueryUnionStatement` with `UnionType = Union` (not `UnionAll`) where the branches are provably disjoint (e.g. literal predicates on the same column with non-overlapping ranges, or different tables with no possible overlap) — or, more conservatively, just flag every plain `UNION` as ignorable advice ("If you don't need duplicate removal, prefer `UNION ALL`").
   - Implementation: `BinaryQueryExpression` with `BinaryQueryExpressionType.Union` and `All == false`. Cheap to detect.
   - Risk: noisy if always-on. Ship ignorable.
   - Suggested ID: `SRP0031` (Performance).

3. **`ORDER BY` without explicit sort direction** (sonar C017)
   - Pattern: `OrderByClause.OrderByElements[i].SortOrder == SortOrder.NotSpecified`.
   - Almost zero-cost check, valuable for readability and intent. Tests should cover OFFSET/FETCH and window-function ORDER BY too.
   - Suggested ID: `SRD0097` (Design) — ignorable.

4. **`ORDER BY RAND()` / `ORDER BY NEWID()`** (sqlcheck 3006)
   - Detect `FunctionCall` named `RAND` or `NEWID` in any `OrderByElement.Expression`.
   - Genuine performance smell on any non-trivial table. Trivial to implement; existing function-name visitors apply.
   - Suggested ID: `SRP0032` — ignorable.

### Tier 2 — worth implementing, but secondary

5. **Comma-style cartesian join (`FROM a, b`)** (sonar C023)
   - Pattern: a `FromClause` where multiple `NamedTableReference` siblings appear directly under `TableReferences` (i.e. comma-separated rather than `INNER JOIN ... ON`). Differs from `SRD0020` which targets missing/incomplete `ON` predicates on explicit joins.
   - Suggested ID: `SRD0098` — ignorable; should not flag deliberate `CROSS JOIN` written with explicit keyword.

6. **`HAVING` without aggregate (should be `WHERE`)** (sqlcheck 3012)
   - Pattern: `QuerySpecification.HavingClause` whose predicate references no aggregate function (`SUM`, `COUNT`, `AVG`, `MIN`, `MAX`, etc.) and no grouped expression — those predicates are evaluable in `WHERE`.
   - Implementation: aggregate-name list already exists in `Globals/Constants.cs::Aggregates`. Walk the `HavingClause` searching for `FunctionCall` whose name is in that list; if none found, raise.
   - Suggested ID: `SRP0033`.

### Tier 3 — opinionated, only as opt-in (default off, ignorable)

7. **Generic primary-key column name `Id`** (sqlcheck 1004)
   - Many shops prefer `<TableName>Id`. Worth offering but contentious. Suggest shipping as ignorable, **not** triggered by AdventureWorks-style schemas during smoke tests.

8. **NULL-unsafe string concatenation** (sqlcheck 3004)
   - `a + b` where either operand is a nullable column and `CONCAT_NULL_YIELDS_NULL` is ON (which `SRD0084` already requires). Recommend `CONCAT(...)` or wrap each operand in `ISNULL(...,'')`/`COALESCE(...,'')`.
   - Requires nullability info — leverage existing `GetDataType` / model lookup helpers in `BaseSqlCodeAnalysisRule`. More expensive; defer.

9. **`DISTINCT` over a join** (sqlcheck 3016)
   - Frequently a smell, frequently legitimate. Low signal-to-noise unless paired with table-cardinality heuristics. Skip for now.

## Explicit non-candidates

These are listed only to document that they were considered and rejected:

- **C022 — non-materialized view used.** Indexed views in SQL Server have many restrictions; cannot recommend categorically.
- **C030 — missing file header.** Rules run against the DacFx semantic model, not against text files. There is no "file" concept by analysis time.
- **1001/1006/1007 (multi-valued attribute, EAV, metadata tribbles).** Design smells that require schema-shape pattern recognition the static rule engine isn't equipped for; high false-positive risk.
- **2002 — values in column definition.** `CHECK (Col IN ('A','B','C'))` is idiomatic T-SQL given the absence of native ENUMs.
- **2003 — files as data types.** FILESTREAM/BLOB columns are legitimate.
- **2005 — index attribute order.** Needs workload (query) information to evaluate; out of scope for a static rule.
- **3003 — `NOT NULL` everywhere.** Inverted advice; `NOT NULL` is generally good in SQL Server.
- **3005 — `GROUP BY` non-grouped columns.** SQL Server already rejects this at parse/compile time.
- **3008/3013 — spaghetti queries, nested subqueries.** Subjective; modern optimizers handle most.

## Implementation notes for whichever rules are picked up

- New design/perf rules: follow the existing template (`Design/AliasTablesRule.cs` is a clean reference). The const triple is `RuleId` / `RuleDisplayName` / `Message`; the `[ExportCodeAnalysisRule]` attribute carries `Category` from `Globals/Constants.cs` and `RuleScope = SqlRuleScope.Element`.
- New naming rules: model after `Naming/` existing rules; constraint and index names live in different ScriptDom nodes — see `Visitors/CreateIndexStatementVisitor.cs` and traverse `CreateTableStatement.Definition.TableConstraints`.
- Mark each rule **Ignorable** unless violation is universally a defect. The convention is `<IsIgnorable>true</IsIgnorable>` in the class XML doc comment; the docs generator reads this.
- Add SQL fixtures under `sqlprojects/TSQLSmellsTest/` and corresponding `SR####Tests.cs` under `test/SqlServer.Rules.Test/<Category>/`. Smoke-test fixtures (AW, Chinook, Fabric) likely need their `ExpectedProblems` lists updated when a new broadly-applicable rule lands.
- Do not edit anything under `docs/` directly — re-run `SqlServer.Rules.DocsGenerator` after the implementation lands so the published rule list updates.

## Suggested next step

Open one issue per Tier-1 rule (or a single tracking issue with checkboxes) so they can be implemented independently. Tier-1 rules 2–4 are each ~50 lines of rule code + a fixture; the naming-prefix family (rule 1) is larger and benefits from being designed as a single configurable rule rather than six near-duplicates.
