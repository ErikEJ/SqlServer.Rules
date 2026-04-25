# TSQLSmells Package Analysis: Comparison with SqlServer.Rules

This document compares the TSQLSmells (SML) and SqlServer.Rules (SRD/SRP/SRN) libraries, identifies overlapping rules, and recommends candidates for removal or porting.

## Summary

| Category | Count |
|----------|-------|
| TSQLSmells rules (SML) | 47 |
| SqlServer.Rules Design rules (SRD) | 63 |
| SqlServer.Rules Performance rules (SRP) | 24 |
| SqlServer.Rules Naming rules (SRN) | 4 |
| **Direct overlaps** (same check in both) | **16** |
| **SML rules with no equivalent** | **29** |

## Overlapping Rules (Candidates for Removal from TSQLSmells)

These TSQLSmells rules check for the same or nearly identical issues already covered by SqlServer.Rules and are candidates for removal.

| SML Rule | SML Description | SqlServer.Rules Equivalent | SR Description | Notes |
|----------|----------------|---------------------------|----------------|-------|
| SML002 | Best practice is to use two part naming | SRD0039 / SRN0006 | Object not schema qualified / Use of default schema | SRD0039 covers DML references; SRN0006 covers object creation |
| SML003 | Dirty reads cause consistency errors (NOLOCK hint) | SRD0034 | Use of NOLOCK | Both flag NOLOCK table hints |
| SML004 | Don't override the optimizer | SRD0030 | Avoid use of HINTS | SRD0030 covers all hints broadly |
| SML005 | Avoid use of SELECT * | SRD0006 | Avoid SELECT * | Direct overlap |
| SML007 | Avoid use of ordinal positions in ORDER BY | SRD0025 | Avoid ORDER BY with numbers | Direct overlap |
| SML010 | READ UNCOMMITTED: dirty reads cause consistency errors | SRD0034 | Use of NOLOCK | SRD0034 covers NOLOCK; SML010 covers SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED |
| SML012 | Missing column specifications on INSERT | SRD0015 | Implicit column list | Direct overlap |
| SML021 | Use two part naming in EXECUTE statements | SRD0039 | Object not schema qualified | SRD0039 covers EXECUTE statements |
| SML024 | Use two part naming | SRN0006 | Use of default schema | SML024 checks object definition naming; SRN0006 is the same check |
| SML030 | Include SET NOCOUNT ON inside stored procedures | SRP0005 | SET NOCOUNT ON recommended | SRP0005 also covers triggers |
| SML031 | EXISTS/NOT EXISTS can be more performant than COUNT(*) | SRP0023 | Enumerating for existence check | Direct overlap |
| SML042 | Use of SET ROWCOUNT is deprecated: use TOP | SRD0036 | Do not use SET ROWCOUNT | Direct overlap |
| SML044 | Don't override the optimizer (FORCESCAN) | SRD0030 | Avoid use of HINTS | SRD0030 covers FORCESCAN hints |
| SML045 | Don't override the optimizer (Index Hint) | SRD0030 | Avoid use of HINTS | SRD0030 covers index hints |
| SML046 | "= NULL" comparison | SRD0011 | Equality compare with NULL | Direct overlap |
| SML047 | Use of deprecated data type | SRD0051 | Do not use deprecated types | Both flag TEXT, NTEXT, IMAGE |

## Near-Overlaps (Partially Covered)

These SML rules have related but not identical coverage in SqlServer.Rules.

| SML Rule | SML Description | Related SR Rule | SR Description | Gap |
|----------|----------------|-----------------|----------------|-----|
| SML017 | ARITHABORT should be ON | SRD0069 | Use SET XACT_ABORT ON with explicit transactions | Different SET options; SML017 checks ARITHABORT specifically, SRD0069 checks XACT_ABORT |
| SML022 | Identity value should be agnostic | SRD0056 | Unsafe identity retrieval (avoid @@IDENTITY) | SML022 flags SET IDENTITY_INSERT; SRD0056 flags @@IDENTITY usage |

## Unique TSQLSmells Rules (Candidates for Porting to SqlServer.Rules)

These rules exist only in TSQLSmells and have no equivalent in SqlServer.Rules. They are candidates for porting.

### High Priority (Valuable Best Practices)

| SML Rule | Description | Rationale for Porting |
|----------|-------------|----------------------|
| SML001 | Avoid cross-server joins | Detects linked server joins which cause performance issues |
| SML006 | Avoid explicit conversion of columnar data | Prevents unnecessary type conversions in queries |
| SML011 | Single-character aliases are poor practice | Improves code readability |
| SML033 | Single-character variable names are poor practice | Improves code readability |
| SML034 | Expression used with TOP should be wrapped in parentheses | Prevents syntax issues with complex TOP expressions |
| SML035 | TOP(100) PERCENT is ignored by the optimizer | Detects common misconception about TOP usage |
| SML043 | Potential SQL injection issue | Security-critical detection |

### Medium Priority (SET Option Validation)

These rules validate SQL Server session settings that can affect query behavior and indexed view compatibility.

| SML Rule | Description | Rationale for Porting |
|----------|-------------|----------------------|
| SML008 | Don't change DATEFORMAT | Changing DATEFORMAT can cause date parsing issues |
| SML009 | Don't change DATEFIRST | Changing DATEFIRST affects date calculations |
| SML013 | CONCAT_NULL_YIELDS_NULL should be ON | Required for indexed views and computed columns |
| SML014 | ANSI_NULLS should be ON | Required for indexed views; affects NULL comparisons |
| SML015 | ANSI_PADDING should be ON | Required for indexed views; affects trailing spaces |
| SML016 | ANSI_WARNINGS should be ON | Required for indexed views; affects error behavior |
| SML018 | NUMERIC_ROUNDABORT should be OFF | Required for indexed views |
| SML019 | QUOTED_IDENTIFIER should be ON | Required for indexed views and filtered indexes |
| SML020 | FORCEPLAN should be OFF | Forces legacy join order behavior |

### Lower Priority (Convention-Based or Niche)

| SML Rule | Description | Rationale |
|----------|-------------|-----------|
| SML023 | Avoid single-line comments | Style preference; may be too opinionated |
| SML025 | RANGE windows are slower than ROWS (explicit use) | Performance pattern for window functions |
| SML026 | RANGE windows are slower than ROWS (implicit use) | Performance pattern for window functions |
| SML027 | CREATE TABLE should specify schema | Related to SRN0006 but specifically for CREATE TABLE |
| SML028 | Ordering in a view does not guarantee result set ordering | Important misconception to detect |
| SML029 | Cursors default to writable; specify FAST_FORWARD | Cursor performance optimization |
| SML032 | Ordering in a derived table does not guarantee ordering | Important misconception to detect |
| SML036 | Foreign key constraints should be named | Constraint naming convention |
| SML037 | Check constraints should be named | Constraint naming convention |
| SML038 | PK constraints on temp tables should not be named | Prevents temp table contention |
| SML039 | Default constraints on temp tables should not be named | Prevents temp table contention |
| SML040 | FK constraints on temp tables should not be named | Prevents temp table contention |
| SML041 | Check constraints on temp tables should not be named | Prevents temp table contention |

## Complete TSQLSmells Rule Inventory

| Rule | Description | Status |
|------|-------------|--------|
| SML001 | Avoid cross-server joins | **Unique** – candidate for porting |
| SML002 | Best practice is to use two part naming | **Overlap** – covered by SRD0039, SRN0006 |
| SML003 | Dirty reads cause consistency errors (NOLOCK) | **Overlap** – covered by SRD0034 |
| SML004 | Don't override the optimizer | **Overlap** – covered by SRD0030 |
| SML005 | Avoid use of SELECT * | **Overlap** – covered by SRD0006 |
| SML006 | Avoid explicit conversion of columnar data | **Unique** – candidate for porting |
| SML007 | Avoid ordinal positions in ORDER BY | **Overlap** – covered by SRD0025 |
| SML008 | Don't change DATEFORMAT | **Unique** – candidate for porting |
| SML009 | Don't change DATEFIRST | **Unique** – candidate for porting |
| SML010 | READ UNCOMMITTED: dirty reads | **Overlap** – related to SRD0034 |
| SML011 | Single-character aliases are poor practice | **Unique** – candidate for porting |
| SML012 | Missing column specifications on INSERT | **Overlap** – covered by SRD0015 |
| SML013 | CONCAT_NULL_YIELDS_NULL should be ON | **Unique** – candidate for porting |
| SML014 | ANSI_NULLS should be ON | **Unique** – candidate for porting |
| SML015 | ANSI_PADDING should be ON | **Unique** – candidate for porting |
| SML016 | ANSI_WARNINGS should be ON | **Unique** – candidate for porting |
| SML017 | ARITHABORT should be ON | **Near-overlap** – partially related to SRD0069 |
| SML018 | NUMERIC_ROUNDABORT should be OFF | **Unique** – candidate for porting |
| SML019 | QUOTED_IDENTIFIER should be ON | **Unique** – candidate for porting |
| SML020 | FORCEPLAN should be OFF | **Unique** – candidate for porting |
| SML021 | Use two part naming in EXECUTE statements | **Overlap** – covered by SRD0039 |
| SML022 | Identity value should be agnostic | **Near-overlap** – related to SRD0056 |
| SML023 | Avoid single-line comments | **Unique** – candidate for porting |
| SML024 | Use two part naming | **Overlap** – covered by SRN0006 |
| SML025 | RANGE windows are slower than ROWS (explicit) | **Unique** – candidate for porting |
| SML026 | RANGE windows are slower than ROWS (implicit) | **Unique** – candidate for porting |
| SML027 | CREATE TABLE should specify schema | **Unique** – candidate for porting |
| SML028 | Ordering in a view does not guarantee ordering | **Unique** – candidate for porting |
| SML029 | Cursors default to writable; specify FAST_FORWARD | **Unique** – candidate for porting |
| SML030 | Include SET NOCOUNT ON in stored procedures | **Overlap** – covered by SRP0005 |
| SML031 | EXISTS/NOT EXISTS more performant than COUNT(*) | **Overlap** – covered by SRP0023 |
| SML032 | Ordering in derived table does not guarantee ordering | **Unique** – candidate for porting |
| SML033 | Single-character variable names are poor practice | **Unique** – candidate for porting |
| SML034 | TOP expression should be wrapped in parentheses | **Unique** – candidate for porting |
| SML035 | TOP(100) PERCENT is ignored by the optimizer | **Unique** – candidate for porting |
| SML036 | Foreign key constraints should be named | **Unique** – candidate for porting |
| SML037 | Check constraints should be named | **Unique** – candidate for porting |
| SML038 | PK constraints on temp tables should not be named | **Unique** – candidate for porting |
| SML039 | Default constraints on temp tables should not be named | **Unique** – candidate for porting |
| SML040 | FK constraints on temp tables should not be named | **Unique** – candidate for porting |
| SML041 | Check constraints on temp tables should not be named | **Unique** – candidate for porting |
| SML042 | SET ROWCOUNT is deprecated: use TOP | **Overlap** – covered by SRD0036 |
| SML043 | Potential SQL injection issue | **Unique** – candidate for porting |
| SML044 | Don't override the optimizer (FORCESCAN) | **Overlap** – covered by SRD0030 |
| SML045 | Don't override the optimizer (Index Hint) | **Overlap** – covered by SRD0030 |
| SML046 | "= NULL" comparison | **Overlap** – covered by SRD0011 |
| SML047 | Use of deprecated data type | **Overlap** – covered by SRD0051 |

## Recommendations

### 1. Rules safe to remove from TSQLSmells (direct duplicates)

The following 16 SML rules have direct equivalents in SqlServer.Rules and could be removed:

- SML002, SML003, SML004, SML005, SML007, SML010, SML012, SML021, SML024, SML030, SML031, SML042, SML044, SML045, SML046, SML047

### 2. Rules to port to SqlServer.Rules

The following 29 rules are unique to TSQLSmells and should be considered for porting:

**High priority:** SML001, SML006, SML011, SML033, SML034, SML035, SML043

**Medium priority (SET options):** SML008, SML009, SML013, SML014, SML015, SML016, SML018, SML019, SML020

**Lower priority:** SML023, SML025, SML026, SML027, SML028, SML029, SML032, SML036, SML037, SML038, SML039, SML040, SML041

### 3. Near-overlaps to evaluate case-by-case

- SML017 (ARITHABORT) – partially related to SRD0069 (XACT_ABORT); different SET options
- SML022 (IDENTITY_INSERT) – partially related to SRD0056 (@@IDENTITY); different identity concerns
