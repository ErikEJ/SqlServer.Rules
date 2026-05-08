using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Design
{
    /// <summary>ANSI_NULLS should be ON.</summary>
    /// <FriendlyName>ANSI_NULLS should be ON</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// ANSI_NULLS must be ON for indexed views, indexes on computed columns, and distributed queries.
    /// It also ensures consistent NULL comparison behavior.
    /// </remarks>
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class AnsiNullsOnRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRD0085";
        public const string RuleDisplayName = "ANSI_NULLS should be ON.";
        public const string Message = RuleDisplayName;

        public AnsiNullsOnRule()
            : base(ModelSchema.Procedure, ModelSchema.ScalarFunction, ModelSchema.TableValuedFunction, ModelSchema.DmlTrigger)
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

            var fragment = ruleExecutionContext.ScriptFragment?.GetFragment(
                typeof(CreateProcedureStatement),
                typeof(CreateFunctionStatement),
                typeof(CreateTriggerStatement));

            if (fragment == null)
            {
                return problems;
            }

            var visitor = new PredicateVisitor();
            fragment.Accept(visitor);

            var offenders = from predicate in visitor.NotIgnoredStatements(RuleId)
                            where predicate.Options == SetOptions.AnsiNulls && !predicate.IsOn
                            select predicate;

            problems.AddRange(offenders.Select(predicate => new SqlRuleProblem(MessageFormatter.FormatMessage(Message, RuleId), sqlObj, predicate)));

            return problems;
        }
    }
}
