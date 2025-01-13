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
    /// The comparison expression always evaluates to TRUE or FALSE
    /// </summary>
    /// <FriendlyName>Expression reducible to constaint</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd>
    /// Catches:
    /// ```sql
    /// FROM a
    /// JOIN b ON a.id = b.a_id AND 4 &lt; 3 + 2
    /// ```
    ///
    /// equivalent to
    /// ```sql
    /// FROM a
    /// JOIN b ON a.id = b.a_id /* true */
    /// ```
    /// </ExampleMd>
    /// <remarks>
    /// Checks join and where clause predicates for predicates that will always evaluate to true/false.
    /// </remarks>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class AvoidStaticPredicateCompareRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0050";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "The comparison expression always evaluates to TRUE or FALSE.";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="AvoidStaticPredicateCompareRule"/> class.
        /// </summary>
        public AvoidStaticPredicateCompareRule() : base(ProgrammingAndViewSchemas)
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

            foreach (var select in selectStatementVisitor.Statements)
            {
                var booleanCompareVisitor = new BooleanComparisonVisitor();
                select.Accept(booleanCompareVisitor);

                var offenders =
                    from cmp in booleanCompareVisitor.NotIgnoredStatements(RuleId)
                    where TestCompare(cmp)
                    select cmp;

                problems.AddRange(offenders.Select(t => new SqlRuleProblem(Message, sqlObj, t)));
            }

            var actionStatementVisitor = new ActionStatementVisitor();
            fragment.Accept(actionStatementVisitor);

            foreach (var action in actionStatementVisitor.Statements)
            {
                var booleanCompareVisitor = new BooleanComparisonVisitor();
                action.Accept(booleanCompareVisitor);

                var offenders =
                    from cmp in booleanCompareVisitor.NotIgnoredStatements(RuleId)
                    where TestCompare(cmp)
                    select cmp;

                problems.AddRange(offenders.Select(t => new SqlRuleProblem(Message, sqlObj, t)));
            }

            return problems;
        }

        private static bool TestCompare(BooleanComparisonExpression compare)
        {
            var expr1 = compare.FirstExpression;
            var expr2 = compare.SecondExpression;
            var type1 = expr1.GetType();
            var type2 = expr2.GetType();

            return (
                (
                    (type1 == typeof(IntegerLiteral) || type1 == typeof(StringLiteral))
                    && (type2 == typeof(IntegerLiteral) || type2 == typeof(StringLiteral))
                )
                && Comparer.Equals((expr1 as Literal)?.Value, (expr2 as Literal)?.Value));
        }
    }
}