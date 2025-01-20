using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Performance
{
    /// <summary>Local cursor not closed</summary>
    /// <FriendlyName>Dangling cursor</FriendlyName>
    /// <IsIgnorable>false</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// The rule checks if any local cursor is closed until the end of the batch. Because when open,
    /// the cursor still holds locks on referred-to-tables or views, you should explicitly close it
    /// as soon as it is no longer needed.
    /// </remarks>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Performance,
        RuleScope = SqlRuleScope.Element)]
    public sealed class CursorNotClosedRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRP0007";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Local cursor not closed.";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="CursorNotClosedRule"/> class.
        /// </summary>
        public CursorNotClosedRule()
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

            var openCursorVisitor = new OpenCursorVisitor();
            fragment.Accept(openCursorVisitor);

            if (openCursorVisitor.Count > 0)
            {
                var closeCursorVisitor = new CloseCursorVisitor();
                fragment.Accept(closeCursorVisitor);

                var localOpenCursors = openCursorVisitor.Statements.Where(c => !c.Cursor.IsGlobal);
                var localCloseCursors = closeCursorVisitor.Statements.Where(c => !c.Cursor.IsGlobal);

                var unclosedCursors = localOpenCursors.Where(c =>
                    !localCloseCursors.Any(c2 => Comparer.Equals(c.Cursor.Name.Value, c2.Cursor.Name.Value)));

                foreach (var cursor in unclosedCursors)
                {
                    problems.Add(new SqlRuleProblem(Message, sqlObj, cursor));
                }
            }

            return problems;
        }
    }
}
