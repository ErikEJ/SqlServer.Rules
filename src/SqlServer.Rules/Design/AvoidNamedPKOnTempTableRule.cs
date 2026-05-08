using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Design
{
    /// <summary>
    /// Primary key constraints on temporary tables should not be named.
    /// </summary>
    /// <FriendlyName>Avoid named primary key constraints on temp tables</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class AvoidNamedPKOnTempTableRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRD0092";
        public const string RuleDisplayName = "Primary Key Constraints on temporary tables should not be named.";
        public const string Message = RuleDisplayName;

        public AvoidNamedPKOnTempTableRule()
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

            var visitor = new CreateTableVisitor
            {
                TypeFilter = ObjectTypeFilter.TempOnly,
            };

            fragment.Accept(visitor);

            var offenders = visitor.NotIgnoredStatements(RuleId)
                .Where(t => t.Definition != null)
                .SelectMany(t => t.Definition.TableConstraints.OfType<UniqueConstraintDefinition>())
                .Where(c => c.IsPrimaryKey && c.ConstraintIdentifier != null);

            problems.AddRange(offenders.Select(c =>
                new SqlRuleProblem(MessageFormatter.FormatMessage(Message, RuleId), sqlObj, c)));

            return problems;
        }
    }
}
