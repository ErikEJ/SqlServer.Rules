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
    /// Do not change DATEFIRST in SQL code.
    /// </summary>
    /// <FriendlyName>Avoid changing DATEFIRST</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// Changing DATEFIRST affects date calculations and can produce inconsistent results.
    /// </remarks>
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class AvoidChangeDateFirstRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0083";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Do not change DATEFIRST.";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = "Changing DATEFIRST affects date calculations.";

        /// <summary>
        /// Initializes a new instance of the <see cref="AvoidChangeDateFirstRule"/> class.
        /// </summary>
        public AvoidChangeDateFirstRule()
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

            var visitor = new StatementVisitor();
            fragment.Accept(visitor);

            var offenders = visitor.NotIgnoredStatements(RuleId)
                .OfType<SetCommandStatement>()
                .Where(s => s.Commands.OfType<GeneralSetCommand>().Any(c => c.CommandType == GeneralSetCommandType.DateFirst));

            problems.AddRange(offenders.Select(s =>
                new SqlRuleProblem(MessageFormatter.FormatMessage(Message, RuleId), sqlObj, s)));

            return problems;
        }
    }
}
