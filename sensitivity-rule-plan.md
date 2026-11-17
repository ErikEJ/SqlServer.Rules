# SRD0097: opt-in policy evaluation plan

Status: proposed extension to the implemented SRD0097 rule; policy evaluation is not yet implemented.

Originally investigated: 2026-11-16. Scope revised: 2026-11-17.

## References

- Rule proposal: https://github.com/ErikEJ/SqlServer.Rules/issues/816
- Original background only: https://github.com/microsoft/DacFx/issues/880
- SSMS policy sample: https://github.com/Azure-Samples/sql-data-classification/blob/main/sql_information_protection_default.json
- SQL Data Discovery and Classification: https://learn.microsoft.com/en-us/sql/relational-databases/security/sql-data-discovery-and-classification

## Objective and scope

Extend existing SRD0097 with **explicitly opt-in, policy-based identification of potentially sensitive columns**. A configured SSMS-format policy supplies the naming patterns. Matching columns should be masked or encrypted, unless covered by a policy-evaluation exclusion.

This is static analysis of the supplied model only. No deployment-related features are included: no deployment contributors, source/target comparison, drift detection, Publish/Script blocking, publish-profile properties or DeployReport integration.

Do not implement another classification-only rule or allocate a new rule ID. Preserve SRD0097's existing behaviour when policy evaluation is not selected.

### In scope

- Explicit selection of a local SSMS-format policy file.
- Name-based policy evaluation using column metadata where required by the policy.
- Reuse of SRD0097's masking and Always Encrypted checks.
- Optional exact-column exclusion file for policy-derived findings.
- Configuration discovery for source builds and explicit library/CLI configuration for pathless inputs and DACPACs.
- Existing rule diagnostics, error handling and severity controls.
- Editor configuration context only where needed to support the same opt-in feature.

### Out of scope

- Any deployment feature or production-state comparison.
- Changes to existing classification detection, its default enablement or its exclusions.
- A new global minimum-rank setting or classification/rank-only rule expansion.
- Automatic policy selection, online policy retrieval or bundled default-policy distribution.
- New audit/report formats, informational exception diagnostics or custom build reporting tasks.
- Public web API policy uploads or server-local path configuration.
- Hard-coded domain dictionaries, transliteration modes or generic suffix exclusions.

This is a guardrail, not proof of security or GDPR compliance. Masking does not prevent privileged access or inference attacks.

## Existing implementation baseline

Reviewed on 2026-11-17:

- `src\SqlServer.Rules\Design\UnprotectedSensitiveColumnsRule.cs` implements SRD0097 as a table-scoped rule.
- It reports columns with a non-empty sensitivity label or information type when their masking function is absent and encryption type is zero.
- It uses existing table/column whitelist handling and reports problems against the column.
- `test\SqlServer.Rules.Test\Design\SRD0097Tests.cs` includes `FlagsClassifiedUnprotectedColumnOnly`, using `sqlprojects\TSQLSmellsTest\SRD0097SensitiveColumns.sql`.

These checks are the compatibility baseline, not work to recreate. Rank-only/ID-only classification changes and broader temporal/history handling are separate work, not prerequisites for policy evaluation.

Earlier in-memory probes against DacFx 170.5.96 confirmed classification, masking and Always Encrypted metadata access. They did not verify SSMS matching parity or complete host integration.

## Evaluation contract

1. Run existing classification-based SRD0097 evaluation unchanged.
2. If no policy is explicitly selected, do not perform policy matching or read an exclusion file.
3. If a policy is selected, validate configuration and load a consistent snapshot for the analysis.
4. Evaluate eligible columns against enabled policy information types and keywords.
5. Apply existing protection checks: masked or Always Encrypted columns do not produce protection findings.
6. Apply valid exact-column exclusions to policy-derived findings only.
7. Report at most one SRD0097 finding per column across classification and policy paths.

An exclusion or policy keyword exclusion must never suppress an existing classification-derived finding. If both paths qualify, retain the existing classification diagnostic and do not add a duplicate policy diagnostic.

Policy-derived diagnostics should identify the column and matched information type/pattern, describe it as potentially sensitive, and recommend masking, Always Encrypted or a documented policy exclusion. Do not claim that a policy match adds classification metadata to the model.

Use existing host severity and rule-disable controls. Selecting a policy does not override an explicitly disabled SRD0097 rule. Opt-in applies to the new policy evaluation, not to the existing classification-based rule.

## Policy format and matching

Accept an explicitly supplied local file in the SSMS SQL Information Protection format. Users may check the Microsoft sample into their own repository and select it; there is no automatic default or download.

At the original investigation, the sample version 2.0 policy contained 12 information types and 366 keywords. Relevant fields include labels and enabled flags, information types and recommended-label references, and keyword `Pattern`, `CanBeNumeric` and `Excluded` values. Retain ranks as policy metadata where useful for diagnostics; do not add rank filtering in this scope.

Patterns are SQL-LIKE-style, not regular expressions. The sample contains `%[^h]ip%address%`.

### Compatibility spike

Establish and document:

- `%`, `_`, bracket expressions, negation and literal escaping.
- Case and accent behaviour.
- Numeric-column eligibility and alias types, reusing cached type-resolution helpers.
- Disabled labels/information types and invalid label references.
- Deterministic handling of overlapping matches.
- The shape and semantics of non-null keyword `Excluded` values.
- Supported policy versions and handling of unsupported matching-affecting fields.

All sample keyword `Excluded` values inspected originally were null. Use an SSMS-exported custom policy with exclusions to establish behaviour before claiming parity. If a construct cannot be supported faithfully, reject it with an explicit configuration error rather than silently ignoring it.

Patterns for Nordic names and domain abbreviations belong in the configured policy. Do not hard-code global exclusions for `username`, `password`, `ipaddress` or `macaddress`: Microsoft's sample deliberately includes these categories. Do not introduce suffix shortcuts that hide `NationalId` or `TaxId`, or silently normalise away meaningful pattern characters.

## Optional policy-exclusion file

Retain the upstream JSON shape as a local static-analysis configuration file:

```json
{
  "exceptions": [
    {
      "column": "[dbo].[Hotel].[ContactName]",
      "reason": "Hotel reception desk, not a natural person",
      "approvedBy": "data-protection-team",
      "expires": "2027-06-30"
    }
  ]
}
```

Semantics:

- Exclude only the policy-derived evaluation for the named column, never the existing classification check.
- Require an exact schema/table/column identity and non-empty reason; no wildcards.
- Parse quoted SQL identifiers correctly, including embedded dots and escaped brackets.
- Use identifier comparison consistent with the analysed model.
- Allow optional `approvedBy` and `expires` metadata.
- Interpret `expires` as an ISO date valid through that UTC calendar date; expired entries no longer exclude a policy finding.
- Reject invalid identifiers, malformed dates and conflicting duplicates explicitly.
- Keep column exclusions distinct from keyword exclusions inside the policy.

Reasons remain documented in the source-controlled exclusion file. This scope does not promise to print excluded findings or reasons in every host, add an audit result collection, or modify report schemas.

Configuring an exclusion file without a selected policy is an explicit configuration error, not a way to suppress classification findings.

## Configuration and integration

Illustrative source-build configuration, subject to API validation:

```ini
[*.sql]
sqlserver_rules.srd0097.policy_file = sensitivity-policy.json
sqlserver_rules.srd0097.exceptions_file = sensitivity-exceptions.json
```

`policy_file` is the opt-in switch. There is no separate enable flag or implicit default. Removing the policy setting restores existing SRD0097 behaviour.

- Resolve relative paths against the EditorConfig file defining each setting, not the process working directory.
- Preserve setting origins through inherited/nested EditorConfig sections.
- Explicit library/CLI configuration takes precedence over discovered configuration; validate the resulting policy/exclusion pair.
- Missing, unreadable, malformed or unsupported explicitly selected files must produce visible analysis errors, not fallback to a clean classification-only result.
- Snapshot settings and file contents per analysis; avoid stale permanent caches and process-wide mutable state.
- Reevaluate expiry and observe file changes in long-lived hosts without cross-project leakage.

### Reuse existing surfaces

- `Naming\NamingRuleRegexConfiguration.cs` and `EditorConfig.Core\`: discovery precedent; reuse suitable infrastructure without inheriting silent error fallbacks or stale-cache behaviour.
- `BaseSqlCodeAnalysisRule.cs`: existing data-type resolution helpers.
- `AnalyzerOptions.cs`, `AnalyzerFactory.cs`, CLI options and program mapping: explicit policy/exclusion paths and configuration base where needed.
- DacFx rule descriptors expose discovered rule instances; verify per-service configuration lifecycle before selecting that approach. Native DacFx rule settings do not carry arbitrary policy file paths.
- For DACPACs and raw SQL text, use explicit options or an explicit configuration context; do not rely on embedded source paths or ambient working directories.
- Server requests and editor clients need document/configuration context for unsaved content. Limit protocol/UI changes to carrying this feature's settings or context.
- Keep the public web host unchanged and do not expose server-local file paths to public callers.

Verify actual rules-package versions and dependency loading during implementation; do not assume repository-owned hosts consume the source rules assembly automatically.

## Implementation phases

### 1. Confirm matching and configuration feasibility

- Verify SSMS matching semantics using the default sample and exported custom policies.
- Verify EditorConfig path origins under supported SQL build hosts.
- Verify explicit per-analysis settings for CLI/library, DACPACs and text inputs.
- Validate serializer/assembly-loading options for netstandard2.1 and net472 before adding dependencies.

Acceptance gate: documented supported semantics and configuration contract, with no silent unsupported matching behaviour.

### 2. Implement opt-in policy evaluation in SRD0097

- Add focused policy reader/matcher helpers and per-analysis configuration.
- Reuse current column selection, whitelist and protection behaviour.
- Add policy-derived findings without changing the existing classification path.
- Deduplicate findings across both paths.
- Implement exact exclusions, reasons, optional expiry and explicit configuration errors.

### 3. Wire supported hosts

- Add source-build discovery and explicit library/CLI options.
- Preserve configuration context for editor/server-mode inputs where needed.
- Ensure errors reach existing host error channels and CLI failures return non-zero status.
- Verify the updated rule package is actually loaded by tooling.
- Do not introduce deployment hooks, audit infrastructure or new report contracts.

### 4. Validate and document

- Run focused rule tests, then the relevant rule/analyzer suites and host-loading checks.
- Exercise actual SQL and DACPAC CLI inputs with and without a selected policy.
- Document opt-in, supported policy syntax, exclusion scope and configuration errors.
- Update rule XML metadata and directly related package documentation; generate rule documentation rather than hand-editing generated files.

## Acceptance tests

### Preserve existing behaviour

- Existing `FlagsClassifiedUnprotectedColumnOnly` remains unchanged and passes.
- With no selected policy, results and diagnostics match the current SRD0097 baseline.
- An unclassified column named `Email` does not gain a finding without opt-in.
- Policy exclusions and disabled policy entries cannot suppress classification-derived findings.
- A column qualifying through both paths produces one existing classification finding.
- Disabling SRD0097 prevents both evaluation paths.

### Policy evaluation

- An unclassified, unprotected matching column gains one actionable policy-derived finding.
- Non-matching columns and protected matching columns do not gain findings.
- Test representative masking functions and deterministic/randomized Always Encrypted.
- Test LIKE wildcards, brackets, negation, escaping, case/accent semantics and multilingual names.
- Test numeric eligibility, alias types, disabled entries and overlapping matches.
- Test non-null policy keyword exclusions against verified SSMS examples.
- Unsupported versions, matching constructs, invalid references and malformed patterns produce explicit errors.

### Exclusions and configuration

- Valid exact exclusions affect only policy-derived findings; reasons remain in the file.
- Test optional metadata, quoted identifiers, blank reasons, malformed dates and conflicting duplicates.
- Test expiry before, on and after the UTC date using a deterministic clock seam.
- An exclusion file without a selected policy produces a configuration error.
- Invalid or inaccessible selected files cannot yield a clean-looking successful analysis.
- Test relative paths, nested setting origins, explicit precedence and non-default working directories.
- Test file edits, expiry reevaluation and concurrent project/request isolation.

### Integration and regression

- Equivalent source SQL and DACPAC inputs produce equivalent policy decisions with explicit configuration.
- Verify supported build hosts and both rule target frameworks.
- Verify editor/text input configuration context where wired.
- Run `SqlServer.Rules.Tests`; run `TSQLAnalyzer.Tests` when analyzer/CLI/protocol wiring changes.
- Capture current sample baselines after the implemented SRD0097; do not assume the earlier 5/45 counts remain current.
- With policy evaluation absent, preserve those current baselines. With it selected, assert exact expected additional findings.

## Remaining decisions

1. Exact verified SSMS pattern/exclusion semantics and compatibility claims.
2. Per-analysis configuration API and explicit-option precedence details.
3. Minimal serializer/dependency approach compatible with both rule target frameworks.
4. Deterministic evidence selection for multiple policy matches.

## Validation status

This revision is documentation-only. The existing SRD0097 implementation and its test were inspected to establish scope. No production changes, builds or test runs were performed for this revision.
