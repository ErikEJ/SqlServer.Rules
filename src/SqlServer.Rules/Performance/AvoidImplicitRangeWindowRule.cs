using System.Collections.Generic;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Performance
{
    /// <summary>Specify ROWS in window functions with ORDER BY to avoid implicit RANGE framing.</summary>
    /// <FriendlyName>Implicit RANGE window frame</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Performance,
        RuleScope = SqlRuleScope.Element)]
    public sealed class AvoidImplicitRangeWindowRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRP0029";
        public const string RuleDisplayName = "Specify ROWS framing explicitly for window functions with ORDER BY to avoid implicit RANGE semantics.";
        public const string Message = RuleDisplayName;

        private static readonly HashSet<string> ExcludedFunctions = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
        {
            "ROW_NUMBER",
            "RANK",
            "DENSE_RANK",
            "NTILE",
            "LAG",
            "LEAD",
        };

        public AvoidImplicitRangeWindowRule()
            : base(ProgrammingAndViewSchemas)
        {
        }

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            var problems = new List<SqlRuleProblem>();
            var sqlObj = ruleExecutionContext.ModelElement;

            if (sqlObj == null || sqlObj.IsWhiteListed())
            {
                return problems;
            }

            var fragment = ruleExecutionContext.ScriptFragment?.GetFragment(ProgrammingAndViewSchemaTypes);
            if (fragment == null)
            {
                return problems;
            }

            var visitor = new FunctionCallVisitor();
            fragment.Accept(visitor);

            foreach (var functionCall in visitor.NotIgnoredStatements(RuleId))
            {
                var functionName = functionCall.FunctionName?.Value;
                if (functionCall.OverClause == null
                    || functionCall.OverClause.OrderByClause == null
                    || functionCall.OverClause.WindowFrameClause != null
                    || string.IsNullOrEmpty(functionName))
                {
                    continue;
                }

                if (ExcludedFunctions.Contains(functionName))
                {
                    continue;
                }

                problems.Add(new SqlRuleProblem(
                    MessageFormatter.FormatMessage(Message, RuleId), sqlObj, functionCall.OverClause));
            }

            return problems;
        }
    }
}
