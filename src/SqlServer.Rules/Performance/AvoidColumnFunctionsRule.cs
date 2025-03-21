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
    /// Avoid wrapping columns within a function in the WHERE clause. This affects sargability.
    /// </summary>
    /// <FriendlyName>Filtering on calculated value</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// When a WHERE clause column is wrapped inside a function, the query optimizer does not see
    /// the column and if an index exists on the column, the index will not to be used.
    /// </remarks>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Performance,
        RuleScope = SqlRuleScope.Element)]
    public sealed class AvoidColumnFunctionsRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRP0009";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Avoid wrapping columns within a function in the WHERE clause. (Sargable)";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="AvoidColumnFunctionsRule"/> class.
        /// </summary>
        public AvoidColumnFunctionsRule()
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

            var whereClauseVisitor = new WhereClauseVisitor();
            fragment.Accept(whereClauseVisitor);

            foreach (var whereClause in whereClauseVisitor.Statements)
            {
                var functionVisitor = new FunctionCallVisitor();
                whereClause.Accept(functionVisitor);

                var offenders = from FunctionCall f in functionVisitor.NotIgnoredStatements(RuleId)
                                where CheckFunction(f)
                                select f;

                problems.AddRange(offenders.Select(f => new SqlRuleProblem(MessageFormatter.FormatMessage(Message, RuleId), sqlObj, f)));
            }

            return problems;
        }

        private static bool CheckFunction(FunctionCall func)
        {
            if (func == null)
            {
                return false;
            }

            return func.Parameters.OfType<ColumnReferenceExpression>().Any(col =>
            {
                var colId = col.MultiPartIdentifier?.GetObjectIdentifier();
                if (colId == null || colId.Parts.Count == 0)
                {
                    return false;
                }

                return !Constants.DateParts.Contains(colId.Parts.Last(), Comparer);
            }) && !Constants.Aggregates.Contains(func.FunctionName.Value, Comparer);
        }
    }
}
