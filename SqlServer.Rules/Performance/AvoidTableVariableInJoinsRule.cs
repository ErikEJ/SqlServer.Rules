using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Performance
{
    /// <summary>Avoid the use of table variables in join clauses planning and maintenace hazard.</summary>
    /// <FriendlyName>Table variable in JOIN</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    ///   <list type="bullet">
    ///     <item> Execution plan choices may not be optimal or stable when a table variable contains a
    ///       large amount of data ( above 100 rows).</item>
    ///     <item>
    ///       Table variables are not supported in the SQL Server optimizer's cost-based reasoning
    ///       model. Therefore, they should not be used when cost-based choices are required to
    ///       achieve an efficient query plan. Temporary tables are preferred when cost-based
    ///       choices are required. This typically includes queries with joins, parallelism
    ///       decisions, and index selection choices. </item>
    ///     <item> Queries that modify table variables do not generate parallel query execution
    ///       plans. Performance can be affected when very large table variables, or table variables
    ///       in complex queries, are modified. In these situations, consider using temporary tables
    ///       instead. Queries that read table variables without modifying them can still be
    ///       parallelized.</item>
    ///     <item> Indexes cannot be created explicitly on table variables, and no statistics are
    ///       kept on table variables. In some cases, performance may improve by using temporary
    ///       tables instead, which support indexes and statistics.</item>
    ///     <item> CHECK constraints, DEFAULT values and computed columns in the table type
    ///       declaration cannot call user-defined functions.</item>
    ///     <item> Assignment operation between table variables is not supported.</item>
    ///     <item> Because table variables have limited scope and are not part of the persistent
    ///       database, they are not affected by transaction rollbacks.</item>
    ///     <item> Table variables cannot be altered after creation.</item>
    ///   </list>
    /// </remarks>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Performance,
        RuleScope = SqlRuleScope.Element)]
    public sealed class AvoidTableVariableInJoinsRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRP0014";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Avoid the use of table variables in join clauses.";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="AvoidTableVariableInJoinsRule"/> class.
        /// </summary>
        public AvoidTableVariableInJoinsRule()
            : base(ProgrammingSchemas)
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

            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(ProgrammingSchemaTypes);
            var joinVisitor = new JoinVisitor();
            fragment.Accept(joinVisitor);

            foreach (var join in joinVisitor.Statements)
            {
                var tableVarVisitor = new TableVariableVisitor();
                join.Accept(tableVarVisitor);

                problems.AddRange(tableVarVisitor.NotIgnoredStatements(RuleId).Select(tv => new SqlRuleProblem(Message, sqlObj, tv)));
            }

            return problems;
        }
    }
}