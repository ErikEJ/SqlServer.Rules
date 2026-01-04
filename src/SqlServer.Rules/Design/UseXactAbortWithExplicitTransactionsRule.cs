using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Design
{
    /// <summary>Use SET XACT_ABORT ON in stored procedures with explicit transactions</summary>
    /// <FriendlyName>Xact_Abort On</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// This rule scans stored procedures to ensure they SET XACT_ABORT to ON at the
    /// beginning. When SET XACT_ABORT is ON, if a Transact-SQL statement raises a run-time
    /// error, the entire transaction is terminated and rolled back. This setting prevents
    /// transactions from remaining open when certain errors occur.
    /// </remarks>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class UseXactAbortWithExplicitTransactionsRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0069";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Use SET XACT_ABORT ON in stored procedures with explicit transactions.";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="UseXactAbortWithExplicitTransactionsRule"/> class.
        /// </summary>
        public UseXactAbortWithExplicitTransactionsRule()
            : base(ModelSchema.Procedure)
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

            var fragment = ruleExecutionContext.ScriptFragment?.GetFragment(
                typeof(CreateProcedureStatement));

            if (fragment == null)
            {
                return problems;
            }

            var transactionVisitor = new BeginTransactionVisitor();

            fragment.Accept(transactionVisitor);
            if (transactionVisitor.Count == 0)
            {
                return problems;
            }

            var visitor = new PredicateVisitor();
            fragment.Accept(visitor);

            var predicates = from o
                            in visitor.Statements
                             where o.Options == SetOptions.XactAbort && o.IsOn
                             select o;

            var createToken = fragment.ScriptTokenStream.FirstOrDefault(t => t.TokenType == TSqlTokenType.Create);

            if (createToken == null)
            {
                return problems;
            }

            if (!predicates.Any() && Ignorables.ShouldNotIgnoreRule(fragment.ScriptTokenStream, RuleId, createToken.Line))
            {
                problems.Add(new SqlRuleProblem(MessageFormatter.FormatMessage(Message, RuleId), sqlObj));
            }

            return problems;
        }
    }
}
