using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Performance
{
    /// <summary>Avoid cross-server joins.</summary>
    /// <FriendlyName>Avoid cross-server joins</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Performance,
        RuleScope = SqlRuleScope.Element)]
    public sealed class AcoidCrossServerJoinsRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRP0026";
        public const string RuleDisplayName = "Avoid cross-server joins.";
        public const string Message = RuleDisplayName;

        public AcoidCrossServerJoinsRule()
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

            var namedTableVisitor = new NamedTableReferenceVisitor();
            fragment.Accept(namedTableVisitor);

            var offenders = namedTableVisitor.NotIgnoredStatements(RuleId)
                .Where(t =>
                    t.SchemaObject?.ServerIdentifier != null
                    && t.SchemaObject.BaseIdentifier?.Value.Length > 0
                    && t.SchemaObject.BaseIdentifier.Value[0] != '#'
                    && t.SchemaObject.BaseIdentifier.Value[0] != '@');

            problems.AddRange(offenders.Select(t => new SqlRuleProblem(MessageFormatter.FormatMessage(Message, RuleId), sqlObj, t)));

            return problems;
        }
    }
}
