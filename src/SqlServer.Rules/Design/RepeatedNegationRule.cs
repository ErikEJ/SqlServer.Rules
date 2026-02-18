using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Design
{
    /// <summary>Unary prefix operators should not be repeated (NOT NOT).</summary>
    /// <FriendlyName>Repeated NOT operator</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// A double NOT (NOT NOT condition) is confusing and likely a logic error. If the intent is
    /// to negate, use a single NOT. If the intent is no negation, remove both NOTs.
    /// </remarks>
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class RepeatedNegationRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRD0073";
        public const string RuleDisplayName = "Repeated NOT operators found. Simplify the expression.";
        public const string Message = RuleDisplayName;

        public RepeatedNegationRule()
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

            var visitor = new BooleanNotExpressionVisitor();
            fragment.Accept(visitor);

            problems.AddRange(visitor.NotIgnoredStatements(RuleId)
                .Where(notExpr => notExpr.Expression is BooleanNotExpression)
                .Select(notExpr => new SqlRuleProblem(
                    MessageFormatter.FormatMessage(Message, RuleId), sqlObj, notExpr)));

            return problems;
        }
    }
}
