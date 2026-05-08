using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Design
{
    /// <summary>SET CONCAT_NULL_YIELDS_NULL should be ON.</summary>
    /// <FriendlyName>Concat Null Yields Null ON</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// When CONCAT_NULL_YIELDS_NULL is OFF, concatenating a NULL value can produce unexpected
    /// results and can prevent indexed views and indexes on computed columns.
    /// </remarks>
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class ConcatNullYieldsNullOnRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRD0084";
        public const string RuleDisplayName = "SET CONCAT_NULL_YIELDS_NULL should be ON.";
        public const string Message = RuleDisplayName;

        public ConcatNullYieldsNullOnRule()
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

            var visitor = new PredicateVisitor();
            fragment.Accept(visitor);

            problems.AddRange(visitor.NotIgnoredStatements(RuleId)
                .Where(predicate => predicate.Options == SetOptions.ConcatNullYieldsNull && !predicate.IsOn)
                .Select(predicate => new SqlRuleProblem(
                    MessageFormatter.FormatMessage(Message, RuleId),
                    sqlObj,
                    predicate)));

            return problems;
        }
    }
}
