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
    /// TOP(100) PERCENT is ignored by the optimizer.
    /// </summary>
    /// <FriendlyName>TOP(100) PERCENT ignored by optimizer</FriendlyName>
    /// <IsIgnorable>false</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// Using TOP(100) PERCENT is a common misconception. The SQL Server optimizer ignores TOP(100) PERCENT
    /// and does not guarantee any ordering, even when combined with ORDER BY in a subquery or view.
    /// Remove the TOP(100) PERCENT clause or use a meaningful TOP value instead.
    /// </remarks>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class TopHundredPercentRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0081";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "TOP(100) PERCENT is ignored by the optimizer.";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="TopHundredPercentRule"/> class.
        /// </summary>
        public TopHundredPercentRule()
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

            var topVisitor = new TopRowFilterVisitor();
            fragment.Accept(topVisitor);

            var offenders = topVisitor.Statements
                .Where(IsHundredPercent);

            problems.AddRange(offenders.Select(t =>
                new SqlRuleProblem(MessageFormatter.FormatMessage(Message, RuleId), sqlObj, t)));

            return problems;
        }

        private static bool IsHundredPercent(TopRowFilter topFilter)
        {
            if (!topFilter.Percent)
            {
                return false;
            }

            var expression = topFilter.Expression is ParenthesisExpression paren
                ? paren.Expression
                : topFilter.Expression;

            return expression is IntegerLiteral literal && literal.Value == "100";
        }
    }
}
