using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Design
{
    /// <summary>CASE expressions should end with an ELSE clause.</summary>
    /// <FriendlyName>CASE without ELSE</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// A CASE expression without an ELSE clause will silently return NULL when no WHEN condition
    /// matches. This can cause subtle bugs. Always add an explicit ELSE clause to make the
    /// intent clear.
    /// </remarks>
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class CaseWithoutElseRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRD0071";
        public const string RuleDisplayName = "CASE expression should include an ELSE clause.";
        public const string Message = RuleDisplayName;

        public CaseWithoutElseRule()
            : base(ProgrammingAndViewSchemas)
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

            var fragment = ruleExecutionContext.ScriptFragment?.GetFragment(ProgrammingAndViewSchemaTypes);
            if (fragment == null)
            {
                return problems;
            }

            // Check searched CASE (CASE WHEN ... THEN ... END)
            var searchedVisitor = new SearchedCaseExpressionVisitor();
            fragment.Accept(searchedVisitor);

            problems.AddRange(searchedVisitor.NotIgnoredStatements(RuleId)
                .Where(c => c.ElseExpression == null)
                .Select(c => new SqlRuleProblem(MessageFormatter.FormatMessage(Message, RuleId), sqlObj, c)));

            // Check simple CASE (CASE expr WHEN ... THEN ... END)
            var simpleVisitor = new SimpleCaseExpressionVisitor();
            fragment.Accept(simpleVisitor);

            problems.AddRange(simpleVisitor.NotIgnoredStatements(RuleId)
                .Where(c => c.ElseExpression == null)
                .Select(c => new SqlRuleProblem(MessageFormatter.FormatMessage(Message, RuleId), sqlObj, c)));

            return problems;
        }
    }
}
