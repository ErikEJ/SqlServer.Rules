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
    /// QUOTED_IDENTIFIER should be ON.
    /// </summary>
    /// <FriendlyName>Quoted identifier on</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// SET QUOTED_IDENTIFIER OFF can prevent use of indexed views and filtered indexes.
    /// </remarks>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class QuotedIdentifierOnRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0089";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "QUOTED_IDENTIFIER should be ON - Required for indexed views and filtered indexes.";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuotedIdentifierOnRule"/> class.
        /// </summary>
        public QuotedIdentifierOnRule()
            : base(ProgrammingSchemas)
        {
        }

        /// <summary>
        /// Performs analysis and returns a list of problems detected.
        /// </summary>
        /// <param name="ruleExecutionContext">Contains the schema model and model element to analyze.</param>
        /// <returns>
        /// The problems detected by the rule in the given element.
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

            var visitor = new PredicateVisitor();
            fragment.Accept(visitor);

            var offenders = visitor.NotIgnoredStatements(RuleId)
                .Where(o => o.Options == SetOptions.QuotedIdentifier && !o.IsOn);

            problems.AddRange(offenders.Select(o =>
                new SqlRuleProblem(MessageFormatter.FormatMessage(Message, RuleId), sqlObj, o)));

            return problems;
        }
    }
}
