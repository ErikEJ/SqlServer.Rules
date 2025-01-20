using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Performance
{
    /// <summary>Avoid returning results in triggers.</summary>
    /// <FriendlyName>Noisy trigger</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// This rule scans triggers to ensure they do not send data back to the caller.
    /// Applications that modify tables or views with triggers do not necessarily expect results to
    /// be returned as part of the modification operation. For this reason it is not recommended to
    /// return results from within triggers.
    /// </remarks>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Performance,
        RuleScope = SqlRuleScope.Element)]
    public sealed class AvoidReturningResultsFromTriggersRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRP0004";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Avoid returning results in triggers.";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="AvoidReturningResultsFromTriggersRule"/> class.
        /// </summary>
        public AvoidReturningResultsFromTriggersRule()
            : base(ModelSchema.DmlTrigger)
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

            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(
                typeof(CreateTriggerStatement));

            var selectVisitor = new SelectStatementVisitor();
            fragment.Accept(selectVisitor);

            problems.AddRange(selectVisitor.NotIgnoredStatements(RuleId).Select(t => new SqlRuleProblem(Message, sqlObj, t)));

            return problems;
        }
    }
}
