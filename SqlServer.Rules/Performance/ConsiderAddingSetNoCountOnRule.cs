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
    /// <summary>SET NOCOUNT ON is recommended to be enabled in stored procedures and triggers</summary>
    /// <FriendlyName>Noisy trigger</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// This rule scans triggers and stored procedures to ensure they SET NOCOUNT to ON at the
    ///  beginning. Use SET NOCOUNT ON at the beginning of your SQL batches, stored procedures and
    ///  triggers in production environments, as this prevents the sending of DONE_IN_PROC messages
    ///  and suppresses messages like '(1 row(s) affected)' to the client for each statement in a
    ///  stored procedure. For stored procedures that contain several statements that do not return
    ///  much actual data, setting SET NOCOUNT to ON can provide a significant performance boost,
    ///  because network traffic is greatly reduced.
    /// </remarks>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Performance,
        RuleScope = SqlRuleScope.Element)]
    public sealed class ConsiderAddingSetNoCountOnRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRP0005";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "SET NOCOUNT ON is recommended to be enabled in stored procedures and triggers.";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsiderAddingSetNoCountOnRule"/> class.
        /// </summary>
        public ConsiderAddingSetNoCountOnRule() : base(ModelSchema.Procedure, ModelSchema.DmlTrigger)
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

            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(
                typeof(CreateProcedureStatement),
                typeof(CreateTriggerStatement));
            var visitor = new PredicateVisitor();
            fragment.Accept(visitor);

            var predicates = from o
                            in visitor.Statements
                             where o.Options == SetOptions.NoCount && o.IsOn
                             select o;

            var createToken = fragment.ScriptTokenStream.FirstOrDefault(t => t.TokenType == TSqlTokenType.Create);

            if (!predicates.Any() && Ignorables.ShouldNotIgnoreRule(fragment.ScriptTokenStream, RuleId, createToken.Line))
            {
                problems.Add(new SqlRuleProblem(Message, sqlObj));
            }

            return problems;
        }
    }
}
