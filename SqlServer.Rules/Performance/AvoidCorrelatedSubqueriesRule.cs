using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Performance
{
    /// <summary>
    /// Avoid the use of correlated subqueries except for very small tables
    /// </summary>
    /// <FriendlyName>Correlated subquery</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>https://en.wikipedia.org/wiki/Correlated_subquery</remarks>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Performance,
        RuleScope = SqlRuleScope.Element)]
    public sealed class AvoidCorrelatedSubqueriesRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRP0024";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Avoid the use of correlated subqueries except for very small tables.";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="AvoidCorrelatedSubqueriesRule"/> class.
        /// </summary>
        public AvoidCorrelatedSubqueriesRule()
            : base(ProgrammingAndViewSchemas)
        {
        }

        /// <summary>
        /// Performs analysis and returns a list of problems detected
        /// </summary>
        /// <param name="ruleExecutionContext">Contains the schema model and model element to analyze</param>
        /// <returns>
        /// The problems detected by the rule in the given element
        /// </returns>
        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            var problems = new List<SqlRuleProblem>();
            var sqlObj = ruleExecutionContext.ModelElement;
            if (sqlObj == null || sqlObj.IsWhiteListed())
            {
                return problems;
            }

            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(ProgrammingAndViewSchemaTypes);

            var scalarSubqueryVisitor = new ScalarSubqueryVisitor();
            fragment.Accept(scalarSubqueryVisitor);

            var offenders = scalarSubqueryVisitor.NotIgnoredStatements(RuleId).Where(s =>
            {
                var whereClause = (s.QueryExpression as QuerySpecification)?.WhereClause;
                if (whereClause == null)
                {
                    return false;
                }

                var booleanCompares = new BooleanComparisonVisitor();
                whereClause.Accept(booleanCompares);

                foreach (var booleanCompare in booleanCompares.Statements)
                {
                    var colVisitor = new ColumnReferenceExpressionVisitor();
                    booleanCompare.AcceptChildren(colVisitor);
                    if (colVisitor.Count > 1)
                    {
                        return true;
                    }
                }

                return false;
            }).ToList();

            problems.AddRange(offenders.Select(o => new SqlRuleProblem(MessageFormatter.FormatMessage(Message, RuleId), sqlObj, o)));

            return problems;
        }
    }
}
