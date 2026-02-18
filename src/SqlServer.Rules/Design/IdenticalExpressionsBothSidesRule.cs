using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Design
{
    /// <summary>Identical expressions should not be used on both sides of a binary operator.</summary>
    /// <FriendlyName>Identical expressions on both sides</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// Expressions like WHERE col = col or col &lt;&gt; col are almost always bugs.
    /// If comparing a column to itself is intended (e.g., to find non-NULL values),
    /// use IS NOT NULL instead.
    /// </remarks>
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class IdenticalExpressionsBothSidesRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRD0076";
        public const string RuleDisplayName = "Identical expressions on both sides of a comparison operator.";
        public const string Message = RuleDisplayName;

        public IdenticalExpressionsBothSidesRule()
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

            var visitor = new BooleanComparisonVisitor();
            fragment.Accept(visitor);

            foreach (var comparison in visitor.NotIgnoredStatements(RuleId))
            {
                var leftText = GetExpressionText(comparison.FirstExpression);
                var rightText = GetExpressionText(comparison.SecondExpression);

                if (!string.IsNullOrEmpty(leftText)
                    && !string.IsNullOrEmpty(rightText)
                    && Comparer.Equals(leftText, rightText))
                {
                    problems.Add(new SqlRuleProblem(
                        MessageFormatter.FormatMessage(Message, RuleId), sqlObj, comparison));
                }
            }

            return problems;
        }

        private static string GetExpressionText(ScalarExpression expression)
        {
            if (expression is ColumnReferenceExpression colRef)
            {
                return string.Join(".",
                    colRef.MultiPartIdentifier.Identifiers.Select(i => i.Value));
            }

            if (expression is VariableReference varRef)
            {
                return varRef.Name;
            }

            return null;
        }
    }
}
