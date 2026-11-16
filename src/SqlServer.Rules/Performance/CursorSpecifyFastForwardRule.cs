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
        public const string RuleDisplayName = "Use FAST_FORWARD when a cursor is read-only and forward-only to reduce cursor overhead.";
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
                var definition = cursor.CursorDefinition;
                if (definition == null)
                {
                    continue;
                }

                // FAST_FORWARD implies FORWARD_ONLY and READ_ONLY. Only recommend it for cursors that
                // are already read-only and forward-only candidates; flagging SCROLL, updatable, keyset,
                // dynamic or static cursors would change their semantics.
                var options = definition.Options;
                var hasFastForward = options?.Any(o => o.OptionKind == CursorOptionKind.FastForward) == true;
                if (hasFastForward || HasConflictingCursorOption(options) || IsUpdatableCursor(definition))
                {
                    continue;
                }

                problems.Add(new SqlRuleProblem(MessageFormatter.FormatMessage(Message, RuleId), sqlObj, cursor));
            }

            return problems;
        }

        private static bool HasConflictingCursorOption(IList<CursorOption> options)
        {
            return options?.Any(o =>
                o.OptionKind == CursorOptionKind.Scroll
                || o.OptionKind == CursorOptionKind.Keyset
                || o.OptionKind == CursorOptionKind.Dynamic
                || o.OptionKind == CursorOptionKind.Static
                || o.OptionKind == CursorOptionKind.ScrollLocks
                || o.OptionKind == CursorOptionKind.Optimistic
                || o.OptionKind == CursorOptionKind.Insensitive) == true;
        }

        private static bool IsUpdatableCursor(CursorDefinition definition)
        {
            return definition.Select?.QueryExpression?.ForClause is UpdateForClause;
        }
    }
}
