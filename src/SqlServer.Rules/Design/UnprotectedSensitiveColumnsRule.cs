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
                .Where(column =>
                    !string.IsNullOrWhiteSpace(column.GetProperty<string>(Column.SensitivityLabel))
                    || !string.IsNullOrWhiteSpace(column.GetProperty<string>(Column.SensitivityInformationType)))
                .Where(column =>
                    !string.Equals(
                        column.GetProperty<string>(Column.SensitivityLabel),
                        "Public",
                        StringComparison.OrdinalIgnoreCase))
                .Where(column => column.GetProperty<int>(Column.SensitivityRank) != 0)
                .Where(column =>
                    string.IsNullOrWhiteSpace(column.GetProperty<string>(Column.MaskingFunction))
                    && column.GetProperty<int>(Column.EncryptionType) == 0);

            problems.AddRange(columns.Select(column =>
                new SqlRuleProblem(MessageFormatter.FormatMessage(Message, RuleId), column)));

            return problems;
        }
    }
}
