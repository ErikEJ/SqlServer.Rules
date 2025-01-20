using System.Collections.Generic;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Design
{
    /// <summary>
    /// TOP clause used in a query without an ORDER BY clause. Add order by clause to make selection predictable.
    /// </summary>
    /// <FriendlyName>TOP without an ORDER BY</FriendlyName>
    /// <IsIgnorable>false</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// This rule checks for usages of TOP in queries without an ORDER BY clause.
    /// It is generally recommended to specify sort criteria when using TOP clause. Otherwise, the
    /// results produced will be plan dependent and may lead to undesired behavior.
    /// </remarks>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class TopWithoutOrderByRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0014";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "TOP clause used in a query without an ORDER BY clause.";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="TopWithoutOrderByRule"/> class.
        /// </summary>
        public TopWithoutOrderByRule()
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
            var topVisitor = new TopRowFilterVisitor();

            fragment.Accept(topVisitor);

            if (topVisitor.Count > 0)
            {
                var orderByvisitor = new OrderByVisitor();
                fragment.Accept(orderByvisitor);
                if (orderByvisitor.Count < 1)
                {
                    problems.Add(new SqlRuleProblem(Message, sqlObj, topVisitor.Statements[0]));
                }
            }

            return problems;
        }
    }
}