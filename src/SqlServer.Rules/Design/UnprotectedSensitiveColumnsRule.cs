using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Design
{
    /// <summary>
    /// Sensitive columns should be masked or encrypted.
    /// Classifications with a Public label or an explicit RANK = NONE are excluded.
    /// Omitting the rank does not exempt a sensitive classification. A rank alone does not trigger this rule.
    /// </summary>
    /// <FriendlyName>Protect sensitive columns</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class UnprotectedSensitiveColumnsRule : BaseSqlCodeAnalysisRule
    {
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

        public UnprotectedSensitiveColumnsRule()
            : base(ModelSchema.Table)
        {
        }

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            var problems = new List<SqlRuleProblem>();
            var table = ruleExecutionContext.ModelElement;

            if (table == null || table.IsWhiteListed())
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

            var (policyPath, exceptionsPath) = GetPolicyPaths(table);
            if (policyPath != null)
            {
                var policy = SensitivityPolicy.Load(policyPath, exceptionsPath, DateTime.UtcNow);
                var classified = new HashSet<TSqlObject>(classifiedColumns);

                foreach (var column in columns.Where(column => !classified.Contains(column) && !IsProtected(column)))
                {
                if (policy.IsExcluded(column))
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

        private (string? PolicyPath, string? ExceptionsPath) GetPolicyPaths(TSqlObject table)
        {
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
    }
}
