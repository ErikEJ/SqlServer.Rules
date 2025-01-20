using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Design
{
    /// <summary>
    ///
    /// </summary>
    /// <FriendlyName>Avoid using CHARINDEX</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class AvoidCHARINDEXRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0031";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Avoid using CHARINDEX function in WHERE clauses.";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="AvoidCHARINDEXRule"/> class.
        /// </summary>
        public AvoidCHARINDEXRule()
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

            // only visit the function calls inside the where clauses
            var visitor = new WhereClauseVisitor();
            fragment.Accept(visitor);

            foreach (var clause in visitor.Statements)
            {
                var functionVisitor = new FunctionCallVisitor("charindex");
                clause.Accept(functionVisitor);

                problems.AddRange(functionVisitor.NotIgnoredStatements(RuleId).Select(f => new SqlRuleProblem(Message, sqlObj, f)));
            }

            return problems;
        }
    }
}