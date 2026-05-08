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
    /// Ordering in a derived table does not guarantee result set ordering.
    /// </summary>
    /// <FriendlyName>Avoid ORDER BY in derived tables for final ordering</FriendlyName>
    /// <IsIgnorable>false</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// Ordering rows inside a derived table does not guarantee the final outer query result order.
    /// Apply ORDER BY at the outermost query level when deterministic final ordering is required.
    /// </remarks>
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class AvoidOrderByInDerivedTableRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRD0091";
        public const string RuleDisplayName = "Ordering in a derived table does not guarantee result set ordering.";
        public const string Message = RuleDisplayName;

        public AvoidOrderByInDerivedTableRule()
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

            var tableReferenceVisitor = new TableReferenceVisitor();
            fragment.Accept(tableReferenceVisitor);

            var offenders = tableReferenceVisitor.NotIgnoredStatements(RuleId)
                .OfType<QueryDerivedTable>()
                .SelectMany(derivedTable =>
                {
                    var orderByVisitor = new OrderByVisitor();
                    derivedTable.QueryExpression?.Accept(orderByVisitor);
                    return orderByVisitor.Statements;
                });

            problems.AddRange(offenders.Select(offender =>
                new SqlRuleProblem(MessageFormatter.FormatMessage(Message, RuleId), sqlObj, offender)));

            return problems;
        }
    }
}
