using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Design
{
    /// <summary>
    /// Avoid the use of cursors. Try to use set based SQL instead of cursors.
    /// </summary>
    /// <FriendlyName>Avoid Cursors</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class AvoidCursorsRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0033";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Avoid using cursors.";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = "Avoid using cursors.";

        /// <summary>
        /// Initializes a new instance of the <see cref="AvoidCursorsRule"/> class.
        /// </summary>
        public AvoidCursorsRule()
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

            var visitor = new OpenCursorVisitor();
            fragment.Accept(visitor);

            var offenders =
                from c in visitor.NotIgnoredStatements(RuleId)
                select c.Cursor;

            problems.AddRange(offenders.Select(c => new SqlRuleProblem(Message, sqlObj, c)));

            return problems;
        }
    }
}