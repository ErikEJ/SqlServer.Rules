using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Performance
{
    /// <summary>Cursors default to writable. Specify FAST_FORWARD.</summary>
    /// <FriendlyName>Specify FAST_FORWARD for cursors</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// Cursors are writable by default and can incur extra overhead.
    /// For read-only, forward-only cursor scenarios, specify FAST_FORWARD.
    /// </remarks>
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Performance,
        RuleScope = SqlRuleScope.Element)]
    public sealed class CursorSpecifyFastForwardRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRP0030";
        public const string RuleDisplayName = "Cursors default to writable. Specify FAST_FORWARD.";
        public const string Message = RuleDisplayName;

        public CursorSpecifyFastForwardRule()
            : base(ProgrammingSchemas)
        {
        }

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

            var cursorVisitor = new DeclareCursorStatementVisitor();
            fragment.Accept(cursorVisitor);

            foreach (var cursor in cursorVisitor.NotIgnoredStatements(RuleId))
            {
                var hasFastForward = cursor.CursorDefinition?.Options?.Any(o => o.OptionKind == CursorOptionKind.FastForward) == true;
                if (!hasFastForward)
                {
                    problems.Add(new SqlRuleProblem(MessageFormatter.FormatMessage(Message, RuleId), sqlObj, cursor));
                }
            }

            return problems;
        }
    }
}
