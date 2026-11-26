using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Design
{
    /// <summary>
    /// Sensitive columns should be masked or encrypted. An explicitly selected Information Protection policy can also
    /// identify potentially sensitive columns by name.
    /// Classifications with a Public label or an explicit RANK = NONE are excluded.
    /// Omitting the rank does not exempt a sensitive classification. A rank alone does not trigger this rule.
    /// <para>
    ///
    /// Policy evaluation is opt-in. Only SSMS Information Protection policy version 2.0 is supported.
    /// In an applicable `.editorconfig`, set `sqlserver_rules.srd0097.policy_file` to the policy JSON file.
    /// Optionally set `sqlserver_rules.srd0097.exceptions_file` to an exact-column exceptions JSON file. Relative paths are
    /// resolved from the `.editorconfig` file that defines each setting.
    /// </para>
    /// <para>
    ///
    /// Policy keywords use SQL LIKE-style `%`, `_`, and bracket patterns against column names. Matching does not
    /// add classification metadata to the model. `CanBeNumeric: false` skips numeric
    /// types, not `uniqueidentifier` or date/time types. `Excluded: true` ignores only that keyword; it cannot
    /// cancel another match. To avoid matching `SocialSecurityNumberStatusId`, narrow `%social%security%`
    /// to `%social%security%number` (without a trailing wildcard). Similarly, `%dob%` also matches `SavedObjectID`;
    /// use a more specific pattern when that substring is not a date of birth.
    /// </para>
    /// <para>
    ///
    /// Exceptions suppress only policy-derived findings, use exact `[schema].[table].[column]` names, and require
    /// a non-empty `reason`. Optional `approvedBy` records approval metadata for audit trails; optional `expires`
    /// must use `yyyy-MM-dd` and remains valid through that UTC date. An exceptions file requires a policy file.
    /// </para>
    /// <para>
    ///
    /// Dynamic data masking counts as protection for this rule, but only masks query results for callers subject
    /// to masking. It does not encrypt stored data or ensure protection in copies such as Fabric mirroring,
    /// exports, or replicas. Protect those destinations separately.
    /// </para>
    /// </summary>
    /// <FriendlyName>Protect sensitive columns</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd>
    /// Select a policy and optional exceptions file in `.editorconfig`:
    ///
    /// ```ini
    /// [*.sql]
    /// sqlserver_rules.srd0097.policy_file = config/sensitivity-policy.json
    /// sqlserver_rules.srd0097.exceptions_file = config/sensitivity-exceptions.json
    /// ```
    ///
    /// An exceptions file with optional approval and expiration metadata:
    ///
    /// ```json
    /// {
    ///   "exceptions": [
    ///     {
    ///       "column": "[dbo].[Address].[AddressId]",
    ///       "reason": "Surrogate key, not personal data",
    ///       "approvedBy": "Data governance team",
    ///       "expires": "2027-12-31"
    ///     }
    ///   ]
    /// }
    /// ```
    ///
    /// For MSBuild, report SRD0097 findings, including configuration problems, as errors:
    ///
    /// ```xml
    /// &lt;SqlCodeAnalysisRules&gt;+!SqlServer.Rules.SRD0097&lt;/SqlCodeAnalysisRules&gt;
    /// ```
    /// </ExampleMd>
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class UnprotectedSensitiveColumnsRule : BaseSqlCodeAnalysisRule
    {
        private readonly object policyLock = new();
        private readonly Dictionary<string, CachedPolicy> policyCache = new(StringComparer.Ordinal);

        public const string RuleId = Constants.RuleNameSpace + "SRD0097";
        public const string RuleDisplayName = "Classified sensitive columns should be masked or encrypted.";
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Gets or sets an explicitly selected SSMS Information Protection policy file.
        /// </summary>
        public string? PolicyFile { get; set; }

        /// <summary>
        /// Gets or sets an optional exact-column policy exceptions file.
        /// </summary>
        public string? ExceptionsFile { get; set; }

        /// <summary>
        /// Validates explicitly selected policy configuration files.
        /// </summary>
        /// <param name="policyFile">The selected SSMS Information Protection policy file.</param>
        /// <param name="exceptionsFile">The optional exact-column exceptions file.</param>
        public static void ValidatePolicyFiles(string policyFile, string? exceptionsFile)
        {
            if (string.IsNullOrWhiteSpace(policyFile))
            {
                throw new ArgumentException("A sensitivity policy file is required.", nameof(policyFile));
            }

            var fullPolicyPath = Path.GetFullPath(policyFile);
            var fullExceptionsPath = string.IsNullOrWhiteSpace(exceptionsFile)
                ? null
                : Path.GetFullPath(exceptionsFile);

            _ = SensitivityPolicy.Load(fullPolicyPath, fullExceptionsPath);
        }

        public UnprotectedSensitiveColumnsRule()
            : base(ModelSchema.Table)
        {
        }

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            var problems = new List<SqlRuleProblem>();
            var table = ruleExecutionContext.ModelElement;

            if (table == null)
            {
                return problems;
            }

            SensitivityPolicy? policy = null;
            try
            {
                var (policyPath, exceptionsPath) = GetPolicyPaths(table);
                policy = policyPath == null ? null : GetPolicy(policyPath, exceptionsPath);
            }
            catch (Exception exception) when (exception is InvalidOperationException or IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
            {
                problems.Add(new SqlRuleProblem(
                    MessageFormatter.FormatMessage($"Invalid SRD0097 configuration: {exception.Message}", RuleId),
                    table));
            }

            if (table.IsWhiteListed())
            {
                return problems;
            }

            // DacFx uses 0 for explicit NONE and -1 for an omitted rank.
            var columns = table.GetReferenced(DacQueryScopes.All)
                .Where(column => column.ObjectType == Column.TypeClass && !column.IsWhiteListed())
                .ToList();

            var classifiedColumns = columns
                .Where(column =>
                    !string.IsNullOrWhiteSpace(column.GetProperty<string>(Column.SensitivityLabel))
                    || !string.IsNullOrWhiteSpace(column.GetProperty<string>(Column.SensitivityInformationType)))
                .Where(column =>
                    !string.Equals(
                        column.GetProperty<string>(Column.SensitivityLabel),
                        "Public",
                        StringComparison.OrdinalIgnoreCase))
                .Where(column => column.GetProperty<int>(Column.SensitivityRank) != 0)
                .Where(column => !IsProtected(column))
                .ToList();

            problems.AddRange(classifiedColumns.Select(column =>
                new SqlRuleProblem(MessageFormatter.FormatMessage(Message, RuleId), column)));

            if (policy != null)
            {
                var classified = new HashSet<TSqlObject>(classifiedColumns);

                foreach (var column in columns.Where(column => !classified.Contains(column) && !IsProtected(column)))
                {
                if (policy.IsExcluded(column, DateTime.UtcNow))
                {
                    continue;
                }

                var match = policy.FindMatch(column);
                if (match != null)
                {
                    var message = $"Column '{column.Name.Parts.Last()}' is potentially sensitive (policy information type '{match.InformationType}', pattern '{match.Pattern}') but is not masked or Always Encrypted.";
                    problems.Add(new SqlRuleProblem(MessageFormatter.FormatMessage(message, RuleId), column));
                }
                }
            }

            return problems;
        }

        private SensitivityPolicy GetPolicy(string policyPath, string? exceptionsPath)
        {
            var key = SensitivityPolicy.GetCacheKey(policyPath, exceptionsPath);
            var stamp = SensitivityPolicy.GetFileStamp(policyPath, exceptionsPath);

            lock (policyLock)
            {
                if (policyCache.TryGetValue(key, out var cached) && cached.Stamp.Equals(stamp))
                {
                    return cached.Policy;
                }

                var policy = SensitivityPolicy.Load(policyPath, exceptionsPath);
                policyCache[key] = new CachedPolicy(stamp, policy);
                return policy;
            }
        }

        private (string? PolicyPath, string? ExceptionsPath) GetPolicyPaths(TSqlObject table)
        {
            if (EditorConfig != null)
            {
                var policyPath = EditorConfig.GetFilePath("sqlserver_rules.srd0097.policy_file");
                var exceptionsPath = EditorConfig.GetFilePath("sqlserver_rules.srd0097.exceptions_file");
                if (exceptionsPath != null && policyPath == null)
                {
                    throw new InvalidOperationException("SRD0097 exceptions_file requires policy_file.");
                }

                return (policyPath, exceptionsPath);
            }

            if (!string.IsNullOrWhiteSpace(PolicyFile) || !string.IsNullOrWhiteSpace(ExceptionsFile))
            {
                if (string.IsNullOrWhiteSpace(PolicyFile))
                {
                throw new InvalidOperationException("SRD0097 exceptions_file requires policy_file.");
                }

                return (PolicyFile, ExceptionsFile);
            }

            return SensitivityPolicy.GetConfiguredPaths(table);
        }

        private static bool IsProtected(TSqlObject column)
            => !string.IsNullOrWhiteSpace(column.GetProperty<string>(Column.MaskingFunction))
                || column.GetProperty<int>(Column.EncryptionType) != 0;

        private sealed class CachedPolicy
        {
            public CachedPolicy(
                (long PolicyTicks, long PolicyLength, long ExceptionsTicks, long ExceptionsLength) stamp,
                SensitivityPolicy policy)
            {
                Stamp = stamp;
                Policy = policy;
            }

            public (long PolicyTicks, long PolicyLength, long ExceptionsTicks, long ExceptionsLength) Stamp { get; }

            public SensitivityPolicy Policy { get; }
        }
    }
}
