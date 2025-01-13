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
    [ExportCodeAnalysisRule(RuleId,
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
        public const string RuleDisplayName = "When checking for existence use EXISTS instead of COUNT";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;


        /// <summary>
        /// Initializes a new instance of the <see cref="UseExistenceInsteadOfCountRule"/> class.
        /// </summary>
        public UseExistenceInsteadOfCountRule() : base(ProgrammingSchemas)
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

            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(ProgrammingSchemaTypes);

            var ifVisitor = new IfStatementVisitor();
            fragment.Accept(ifVisitor);

            if (ifVisitor.Count == 0) { return problems; }

            foreach (var ifstmt in ifVisitor.Statements)
            {
                var functionVisitor = new FunctionCallVisitor("count");
                ifstmt.Predicate.Accept(functionVisitor);

                if (functionVisitor.Statements.Any() && CheckIf(ifstmt))
                {
                    problems.Add(new SqlRuleProblem(Message, sqlObj, ifstmt));
                }
            }

            return problems;
        }

        private static bool CheckIf(IfStatement ifstmt)
        {
            if (ifstmt.Predicate is BooleanComparisonExpression booleanCompare)
            {
                return (booleanCompare.FirstExpression is IntegerLiteral || booleanCompare.SecondExpression is IntegerLiteral);
            }

            return false;
        }
    }
}
