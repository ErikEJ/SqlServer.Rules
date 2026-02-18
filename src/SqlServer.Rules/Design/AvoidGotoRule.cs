using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Design
{
    /// <summary>GOTO statements should not be used.</summary>
    /// <FriendlyName>Avoid GOTO</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// GOTO creates spaghetti code that is hard to follow and maintain. Use structured
    /// control flow (IF/ELSE, WHILE, TRY/CATCH) instead.
    /// </remarks>
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class AvoidGotoRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRD0070";
        public const string RuleDisplayName = "Avoid using GOTO statements. Use structured control flow instead.";
        public const string Message = RuleDisplayName;

        public AvoidGotoRule()
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

            var visitor = new GoToStatementVisitor();
            fragment.Accept(visitor);

            problems.AddRange(visitor.NotIgnoredStatements(RuleId)
                .Select(s => new SqlRuleProblem(MessageFormatter.FormatMessage(Message, RuleId), sqlObj, s)));

            return problems;
        }
    }
}
