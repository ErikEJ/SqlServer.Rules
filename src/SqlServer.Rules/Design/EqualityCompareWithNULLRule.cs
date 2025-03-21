using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Design
{
    /// <summary>
    /// Equality and inequality comparisons involving a NULL constant found. Use IS NULL or IS NOT NULL.
    /// </summary>
    /// <FriendlyName></FriendlyName>
    /// <IsIgnorable>false</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// This rule scans stored procedures, views, functions and triggers to flag use of equality
    /// and inequality comparisons involving a NULL constant. These comparisons are undefined when
    /// <c>ANSI_NULLS</c> option is set to ON. It is recommended to set <c>ANSI_NULLS</c> to ON and
    /// use the <c>IS</c> keyword to compare against NULL constants. Care must be taken when
    /// comparing null values. The behavior of the comparison depends on the setting of the
    /// <c>SET ANSI_NULLS</c> option. When <c>SET ANSI_NULLS</c> is ON, a comparison in which one
    /// or more of the expressions is NULL does not yield either TRUE or FALSE; it yields UNKNOWN.
    /// This is because a value that is unknown cannot be compared logically against any other value.
    /// This occurs if either an expression is compared to the literal NULL, or if two expressions
    /// are compared and one of them evaluates to NULL.
    /// </remarks>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class EqualityCompareWithNULLRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0011";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Equality and inequality comparisons involving a NULL constant found. Use IS NULL or IS NOT NULL.";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="EqualityCompareWithNULLRule"/> class.
        /// </summary>
        public EqualityCompareWithNULLRule()
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
            var sqlObj = ruleExecutionContext.ModelElement; // proc / view / function
            if (sqlObj == null || sqlObj.IsWhiteListed())
            {
                return problems;
            }

#pragma warning disable CA1031 // Do not catch general exception types
            try
            {
                var fragment = ruleExecutionContext.ScriptFragment?.GetFragment(ProgrammingAndViewSchemaTypes);

                if (fragment == null)
                {
                    return problems;
                }

                // get the combined parameters and declare variables into one search-able list
                var variablesVisitor = new VariablesVisitor();
                fragment.AcceptChildren(variablesVisitor);
                var variables = variablesVisitor.GetVariables();

                var selectStatementVisitor = new SelectStatementVisitor();
                fragment.Accept(selectStatementVisitor);
                foreach (var select in selectStatementVisitor.Statements)
                {
                    if (select.QueryExpression is QuerySpecification query && query.WhereClause != null)
                    {
                        var booleanComparisonVisitor = new BooleanComparisonVisitor();
                        query.WhereClause.Accept(booleanComparisonVisitor);

                        foreach (var comparison in booleanComparisonVisitor.Statements)
                        {
                            if ((comparison.FirstExpression is NullLiteral || comparison.SecondExpression is NullLiteral) &&
                                (comparison.ComparisonType == BooleanComparisonType.Equals
                                || comparison.ComparisonType == BooleanComparisonType.NotEqualToBrackets
                                || comparison.ComparisonType == BooleanComparisonType.NotEqualToExclamation) // probably can remove the ComparisonTypeCheck
                                )
                            {
                                problems.Add(new SqlRuleProblem(MessageFormatter.FormatMessage(Message, RuleId), sqlObj, comparison));
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                // TODO: PROPERLY LOG THIS ERROR
                Debug.WriteLine(ex.ToString());

                // throw;
            }
#pragma warning restore CA1031 // Do not catch general exception types

            return problems;
        }
    }
}