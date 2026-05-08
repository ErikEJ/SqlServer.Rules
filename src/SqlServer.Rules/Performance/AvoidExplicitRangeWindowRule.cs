using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Performance
{
    /// <summary>Avoid explicit RANGE in window frames and prefer ROWS.</summary>
    /// <FriendlyName>Explicit RANGE window frame</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// Explicit RANGE window frames can be slower than equivalent ROWS frames.
    /// Prefer ROWS when row-based framing is intended.
    /// </remarks>
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Performance,
        RuleScope = SqlRuleScope.Element)]
    public sealed class AvoidExplicitRangeWindowRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRP0028";
        public const string RuleDisplayName = "Avoid explicit RANGE window frames; prefer explicit ROWS.";
        public const string Message = RuleDisplayName;

        public AvoidExplicitRangeWindowRule()
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

            var functionVisitor = new FunctionCallVisitor();
            fragment.Accept(functionVisitor);

            var offenders = functionVisitor.NotIgnoredStatements(RuleId)
                .Where(f => f.OverClause?.WindowFrameClause?.WindowFrameType == WindowFrameType.Range);

            foreach (var offender in offenders)
            {
                problems.Add(new SqlRuleProblem(
                    MessageFormatter.FormatMessage(Message, RuleId),
                    sqlObj,
                    offender.OverClause.WindowFrameClause));
            }

            return problems;
        }
    }
}
