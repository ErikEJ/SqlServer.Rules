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
    /// ANSI_PADDING should be ON to avoid subtle behavior differences and support indexed view requirements.
    /// </summary>
    /// <FriendlyName>ANSI_PADDING should be ON</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// This rule flags occurrences of <c>SET ANSI_PADDING OFF</c>.
    /// Keeping ANSI_PADDING ON helps ensure consistent handling of trailing spaces
    /// and is required in scenarios such as indexed views.
    /// </remarks>
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class AnsiPaddingOnRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0086";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "ANSI_PADDING should be ON.";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnsiPaddingOnRule"/> class.
        /// </summary>
        public AnsiPaddingOnRule()
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

            var fragment = ruleExecutionContext.ScriptFragment?.GetFragment(ProgrammingSchemaTypes);
            if (fragment == null)
            {
                return problems;
            }

            var visitor = new PredicateVisitor();
            fragment.Accept(visitor);

            var offenders = visitor.NotIgnoredStatements(RuleId)
                .Where(s => s.Options == SetOptions.AnsiPadding && !s.IsOn);

            problems.AddRange(offenders.Select(s =>
                new SqlRuleProblem(MessageFormatter.FormatMessage(Message, RuleId), sqlObj, s)));

            return problems;
        }
    }
}
