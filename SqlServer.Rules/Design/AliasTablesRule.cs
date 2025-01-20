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
    /// When a query contains multiple tables it is a good practice to alias all tables used in the query.
    /// </summary>
    /// <FriendlyName>Alias Tables Rule</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class AliasTablesRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0038";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Consider aliasing all table sources in the query.";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = "Consider aliasing all table sources in the query.";

        /// <summary>
        /// Initializes a new instance of the <see cref="AliasTablesRule"/> class.
        /// </summary>
        public AliasTablesRule()
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

            var selectStatementVisitor = new SelectStatementVisitor();
            fragment.Accept(selectStatementVisitor);

            if (selectStatementVisitor.Count == 0)
            {
                return problems;
            }

            foreach (var select in selectStatementVisitor.Statements)
            {
                var fromClause = (select.QueryExpression as QuerySpecification)?.FromClause;

                // ignore selects that do not use a from clause with tables
                if (fromClause == null)
                {
                    continue;
                }

                var visitor = new NamedTableReferenceVisitor { TypeFilter = ObjectTypeFilter.PermanentOnly };
                fromClause.Accept(visitor);

                // only scan for aliases if there are more than 1 table in the from clause
                if (visitor.Count <= 1)
                {
                    continue;
                }

                var offenders =
                    from t in visitor.NotIgnoredStatements(RuleId)
                    where t.Alias == null
                    select t;

                problems.AddRange(offenders.Select(t => new SqlRuleProblem(Message, sqlObj, t)));
            }

            return problems;
        }
    }
}