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
    /// Changing DATEFORMAT can cause date parsing issues.
    /// </summary>
    /// <FriendlyName>Do not change DATEFORMAT</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class AvoidChangeDateFormatRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0082";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Changing DATEFORMAT can cause date parsing issues.";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="AvoidChangeDateFormatRule"/> class.
        /// </summary>
        public AvoidChangeDateFormatRule()
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
                .Where(statement => statement.Commands
                    .OfType<GeneralSetCommand>()
                    .Any(command => command.CommandType == GeneralSetCommandType.DateFormat));

            problems.AddRange(offenders.Select(statement =>
                new SqlRuleProblem(MessageFormatter.FormatMessage(Message, RuleId), sqlObj, statement)));

            return problems;
        }
    }
}
