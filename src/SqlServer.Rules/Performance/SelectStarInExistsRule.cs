using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Performance
{
    /// <summary>SELECT statements used as argument of EXISTS should use SELECT 1 instead of SELECT *.</summary>
    /// <FriendlyName>SELECT * in EXISTS</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// Using SELECT * in an EXISTS subquery is unnecessary and unclear. While the optimizer
    /// typically handles this, using SELECT 1 makes intent explicit and is a best practice.
    /// </remarks>
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Performance,
        RuleScope = SqlRuleScope.Element)]
    public sealed class SelectStarInExistsRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRP0025";
        public const string RuleDisplayName = "Use SELECT 1 instead of SELECT * in EXISTS subqueries.";
        public const string Message = RuleDisplayName;

        public SelectStarInExistsRule()
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

            var visitor = new ExistsPredicateVisitor();
            fragment.Accept(visitor);

            foreach (var exists in visitor.NotIgnoredStatements(RuleId))
            {
                if (exists.Subquery?.QueryExpression is QuerySpecification querySpec)
                {
                    var hasSelectStar = querySpec.SelectElements.OfType<SelectStarExpression>().Any();
                    if (hasSelectStar)
                    {
                        problems.Add(new SqlRuleProblem(
                            MessageFormatter.FormatMessage(Message, RuleId), sqlObj, exists));
                    }
                }
            }

            return problems;
        }
    }
}
