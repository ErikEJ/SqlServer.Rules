using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Performance
{
    /// <summary>Reduce number of joined relations.</summary>
    /// <FriendlyName>High join count</FriendlyName>
    /// <IsIgnorable>false</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// Queries that use a high number of joins will cause the compiler to time out trying to find
    ///  a good execution plan. Consider re-arranging the tables in the joins from smallest to
    ///  largest table and applying the FORCE ORDER query hint. 
    /// </remarks>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Performance,
        RuleScope = SqlRuleScope.Element)]
    public sealed class QueryHighJoinCountRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRP0018";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Query uses a high number of joins. ";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = "Query uses {0} joins. This is a high number of joins and can lead to performance issues.";


        /// <summary>
        /// Initializes a new instance of the <see cref="QueryHighJoinCountRule"/> class.
        /// </summary>
        public QueryHighJoinCountRule() : base(ProgrammingAndViewSchemas)
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

            if (sqlObj == null || sqlObj.IsWhiteListed()) { return problems; }

            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(ProgrammingAndViewSchemaTypes);
            var selectStatementVisitor = new SelectStatementVisitor();
            fragment.Accept(selectStatementVisitor);

            foreach (var stmt in selectStatementVisitor.Statements)
            {
                var hasForceOrder = stmt.OptimizerHints?.Any(oh => oh.HintKind == OptimizerHintKind.ForceOrder);
                if (hasForceOrder.GetValueOrDefault(false)) { continue; }

                var querySpecificationVisitor = new QuerySpecificationVisitor();
                stmt.QueryExpression.Accept(querySpecificationVisitor);

                foreach (var query in querySpecificationVisitor.Statements)
                {
                    var fromClause = query.FromClause;
                    if (fromClause == null) { continue; }

                    var namedTableVisitor = new NamedTableReferenceVisitor();
                    fromClause.Accept(namedTableVisitor);

                    var tableCount = namedTableVisitor.Count - 1;

                    if (tableCount > 8)
                    {
                        var msg = string.Format(CultureInfo.InvariantCulture, Message, tableCount);
                        problems.Add(new SqlRuleProblem(msg, sqlObj, fromClause));
                    }
                }
            }


            return problems;
        }
    }
}
