using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Performance
{
    /// <summary>Consider using EXISTS instead of COUNT</summary>
    /// <FriendlyName>Enumerating for existence check</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// COUNT will iterate through every row in the table before returning the result whereas EXISTS
    ///  will stop as soon as records are found.
    /// </remarks>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Performance,
        RuleScope = SqlRuleScope.Element)]
    public sealed class UseExistenceInsteadOfCountRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRP0023";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Use EXISTS for existence checks; COUNT performs unnecessary aggregation.";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="UseExistenceInsteadOfCountRule"/> class.
        /// </summary>
        public UseExistenceInsteadOfCountRule()
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

            var fragment = ruleExecutionContext.ScriptFragment?.GetFragment(ProgrammingSchemaTypes);

            if (fragment == null)
            {
                return problems;
            }

            var ifVisitor = new IfStatementVisitor();
            fragment.Accept(ifVisitor);

            if (ifVisitor.Count == 0)
            {
                return problems;
            }

            foreach (var ifstmt in ifVisitor.Statements)
            {
                var functionVisitor = new FunctionCallVisitor("count");
                ifstmt.Predicate.Accept(functionVisitor);

                if (functionVisitor.Statements.Any() && CheckIf(ifstmt))
                {
                    problems.Add(new SqlRuleProblem(MessageFormatter.FormatMessage(Message, RuleId), sqlObj, ifstmt));
                }
            }

            return problems;
        }

        private static bool CheckIf(IfStatement ifstmt)
        {
            if (ifstmt.Predicate is not BooleanComparisonExpression booleanCompare)
            {
                return false;
            }

            var firstLiteral = booleanCompare.FirstExpression as IntegerLiteral;
            var secondLiteral = booleanCompare.SecondExpression as IntegerLiteral;

            // Exactly one side must be an integer literal; the other side holds the COUNT expression.
            if ((firstLiteral == null) == (secondLiteral == null))
            {
                return false;
            }

            var countExpression = firstLiteral == null ? booleanCompare.FirstExpression : booleanCompare.SecondExpression;
            if (!IsCountStar(countExpression))
            {
                return false;
            }

            var comparisonType = booleanCompare.ComparisonType;
            var literal = (secondLiteral ?? firstLiteral)!;

            if (!int.TryParse(literal.Value, out var value))
            {
                return false;
            }

            // Normalize so the comparison always reads as "count <operator> value".
            if (secondLiteral == null)
            {
                comparisonType = ReverseComparison(comparisonType);
            }

            return IsExistenceComparison(comparisonType, value);
        }

        private static bool IsCountStar(ScalarExpression expression)
        {
            if (expression is not ScalarSubquery { QueryExpression: QuerySpecification querySpecification }
                || querySpecification.SelectElements.Count != 1
                || querySpecification.GroupByClause != null
                || querySpecification.HavingClause != null
                || querySpecification.TopRowFilter != null
                || querySpecification.OffsetClause != null
                || querySpecification.SelectElements[0] is not SelectScalarExpression { Expression: FunctionCall functionCall }
                || functionCall.OverClause != null
                || !string.Equals(functionCall.FunctionName.Value, "count", System.StringComparison.OrdinalIgnoreCase)
                || functionCall.Parameters.Count != 1
                || functionCall.Parameters[0] is not ColumnReferenceExpression { ColumnType: ColumnType.Wildcard })
            {
                return false;
            }

            return true;
        }

        private static BooleanComparisonType ReverseComparison(BooleanComparisonType comparisonType)
        {
            switch (comparisonType)
            {
                case BooleanComparisonType.GreaterThan:
                    return BooleanComparisonType.LessThan;
                case BooleanComparisonType.GreaterThanOrEqualTo:
                    return BooleanComparisonType.LessThanOrEqualTo;
                case BooleanComparisonType.LessThan:
                    return BooleanComparisonType.GreaterThan;
                case BooleanComparisonType.LessThanOrEqualTo:
                    return BooleanComparisonType.GreaterThanOrEqualTo;
                default:
                    return comparisonType;
            }
        }

        private static bool IsExistenceComparison(BooleanComparisonType comparisonType, int value)
        {
            // COUNT(...) is always a non-negative integer, so only the comparisons below are
            // equivalent to EXISTS (at least one row) or NOT EXISTS (no rows).
            switch (comparisonType)
            {
                case BooleanComparisonType.GreaterThan:
                    return value == 0; // count > 0
                case BooleanComparisonType.GreaterThanOrEqualTo:
                    return value == 1; // count >= 1
                case BooleanComparisonType.NotEqualToBrackets:
                case BooleanComparisonType.NotEqualToExclamation:
                    return value == 0; // count <> 0
                case BooleanComparisonType.Equals:
                    return value == 0; // count = 0
                case BooleanComparisonType.LessThan:
                    return value == 1; // count < 1
                case BooleanComparisonType.LessThanOrEqualTo:
                    return value == 0; // count <= 0
                default:
                    return false;
            }
        }
    }
}
