using System.Collections.Generic;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Design
{
    /// <summary>Variables should not be self-assigned.</summary>
    /// <FriendlyName>Variable self-assignment</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// Assigning a variable to itself (SET @x = @x) is a no-op and likely a copy-paste bug.
    /// This rule detects SET statements where the same variable appears on both sides.
    /// </remarks>
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class VariableSelfAssignedRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRD0072";
        public const string RuleDisplayName = "Variable should not be assigned to itself.";
        public const string Message = "Variable '{0}' is assigned to itself.";

        public VariableSelfAssignedRule()
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

            var visitor = new SetVariableStatementVisitor();
            fragment.Accept(visitor);

            foreach (var stmt in visitor.NotIgnoredStatements(RuleId))
            {
                if (stmt.Expression is VariableReference varRef
                    && Comparer.Equals(stmt.Variable.Name, varRef.Name))
                {
                    problems.Add(new SqlRuleProblem(
                        MessageFormatter.FormatMessage(
                            string.Format(System.Globalization.CultureInfo.InvariantCulture, Message, stmt.Variable.Name),
                            RuleId),
                        sqlObj, stmt));
                }
            }

            return problems;
        }
    }
}
