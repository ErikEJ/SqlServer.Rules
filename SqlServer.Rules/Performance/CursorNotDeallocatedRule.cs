using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Performance
{
    /// <summary>Local cursor not explicitly deallocated</summary>
    /// <FriendlyName>Unfreed cursor</FriendlyName>
    /// <IsIgnorable>false</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Performance,
        RuleScope = SqlRuleScope.Element)]
    public sealed class CursorNotDeallocatedRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRP0008";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Local cursor not explicitly deallocated.";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;


        /// <summary>
        /// Initializes a new instance of the <see cref="CursorNotDeallocatedRule"/> class.
        /// </summary>
        public CursorNotDeallocatedRule() : base(ProgrammingSchemas)
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

            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(ProgrammingSchemaTypes);

            var openCursorVisitor = new OpenCursorVisitor();
            fragment.Accept(openCursorVisitor);

            if (openCursorVisitor.Count > 0)
            {
                var deallocateCursorVisitor = new DeallocateCursorVisitor();
                fragment.Accept(deallocateCursorVisitor);

                var localOpenCursors = openCursorVisitor.Statements.Where(c => !c.Cursor.IsGlobal);
                var localDeallocateCursors = deallocateCursorVisitor.Statements.Where(c => !c.Cursor.IsGlobal);

                var unDeallocatedCursors = localOpenCursors.Where(c =>
                    !localDeallocateCursors.Any(c2 => Comparer.Equals(c.Cursor.Name.Value, c2.Cursor.Name.Value)));

                foreach (var cursor in unDeallocatedCursors)
                {
                    problems.Add(new SqlRuleProblem(Message, sqlObj, cursor));
                }
            }

            return problems;
        }
    }
}
