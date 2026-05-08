using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Design
{
    /// <summary>
    /// Expressions used with TOP should be wrapped in parentheses.
    /// </summary>
    /// <FriendlyName>TOP expression should use parentheses</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// Wrapping TOP expressions in parentheses avoids syntax issues and makes complex TOP clauses
    /// clearer and more consistent.
    /// </remarks>
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class TopExpressionParenthesesRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRD0080";
        public const string RuleDisplayName = "Expression used with TOP should be wrapped in parentheses.";
        public const string Message = RuleDisplayName;

        public TopExpressionParenthesesRule()
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

            var visitor = new TopRowFilterVisitor();
            fragment.Accept(visitor);

            var offenders = visitor.NotIgnoredStatements(RuleId)
                .Where(top => top.Expression is not ParenthesisExpression);

            problems.AddRange(offenders.Select(top =>
                new SqlRuleProblem(MessageFormatter.FormatMessage(Message, RuleId), sqlObj, top)));

            return problems;
        }
    }
}
