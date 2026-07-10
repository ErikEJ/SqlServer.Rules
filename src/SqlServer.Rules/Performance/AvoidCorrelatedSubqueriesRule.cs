using System;
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

            var fragment = ruleExecutionContext.ScriptFragment?.GetFragment(ProgrammingAndViewSchemaTypes);

            if (fragment == null)
            {
                return problems;
            }

            var scalarSubqueryVisitor = new ScalarSubqueryVisitor();
            var queryStatementVisitor = new QueryStatementVisitor();
            fragment.Accept(scalarSubqueryVisitor);
            fragment.Accept(queryStatementVisitor);

            var offenders = scalarSubqueryVisitor.NotIgnoredStatements(RuleId).Where(s =>
            {
                if (ReferencesOnlyCtes(s, queryStatementVisitor.Statements))
                {
                    return false;
                }

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

        private static bool ReferencesOnlyCtes(ScalarSubquery scalarSubquery, IEnumerable<StatementWithCtesAndXmlNamespaces> queryStatements)
        {
            if (scalarSubquery == null)
            {
                throw new ArgumentNullException(nameof(scalarSubquery));
            }

            if (queryStatements == null)
            {
                throw new ArgumentNullException(nameof(queryStatements));
            }

            var containingStatement = queryStatements
                .Where(statement => statement.WithCtesAndXmlNamespaces?.CommonTableExpressions?.Count > 0 && ContainsFragment(statement, scalarSubquery))
                .OrderBy(statement => statement.FragmentLength)
                .FirstOrDefault();

            if (containingStatement?.WithCtesAndXmlNamespaces == null)
            {
                return false;
            }

            var namedTableVisitor = new NamedTableReferenceVisitor();
            scalarSubquery.Accept(namedTableVisitor);
            if (namedTableVisitor.Count == 0)
            {
                return false;
            }

            var cteNames = containingStatement.WithCtesAndXmlNamespaces.CommonTableExpressions
                .Select(expression => expression.ExpressionName?.Value)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            return cteNames.Count > 0
                && namedTableVisitor.Statements.All(table => cteNames.Contains(table.SchemaObject.BaseIdentifier.Value));
        }

        private static bool ContainsFragment(TSqlFragment outerFragment, TSqlFragment innerFragment)
        {
            if (outerFragment == null || innerFragment == null || outerFragment.StartOffset < 0 || innerFragment.StartOffset < 0)
            {
                return false;
            }

            var outerEnd = outerFragment.StartOffset + outerFragment.FragmentLength;
            var innerEnd = innerFragment.StartOffset + innerFragment.FragmentLength;

            return outerFragment.StartOffset <= innerFragment.StartOffset && innerEnd <= outerEnd;
        }
    }
}
