using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Design
{
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class AvoidDeletesWithoutWhereRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0017";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "DELETE statement without row limiting conditions.";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="AvoidDeletesWithoutWhereRule"/> class.
        /// </summary>
        public AvoidDeletesWithoutWhereRule()
            : base(ProgrammingSchemas)
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

            if (sqlObj == null || sqlObj.IsWhiteListed())
            {
                return problems;
            }

            var fragment = ruleExecutionContext.ScriptFragment?.GetFragment(ProgrammingSchemaTypes);
            var visitor = new DeleteVisitor();

            if (fragment == null)
            {
                return problems;
            }

            fragment.Accept(visitor);

            foreach (var stmt in visitor.NotIgnoredStatements(RuleId))
            {
                if (stmt.DeleteSpecification.WhereClause != null
                    || !(stmt.DeleteSpecification.Target is NamedTableReference reference))
                {
                    continue;
                }

                var tableName = reference.SchemaObject.Identifiers.Last().Value;

                if (stmt.DeleteSpecification.FromClause != null)
                {
                    var tableVisitor = new TableReferenceVisitor();
                    stmt.DeleteSpecification.FromClause.Accept(tableVisitor);

                    var table = tableVisitor.Statements.OfType<NamedTableReference>()
                        .FirstOrDefault(t => Comparer.Equals(t.Alias?.Value, tableName));
                    if (table != null)
                    {
                        tableName = table.SchemaObject.Identifiers.Last().Value;
                    }
                }

                if (!(tableName.StartsWith('#') || tableName.StartsWith('@')))
                {
                    problems.Add(new SqlRuleProblem(MessageFormatter.FormatMessage(Message, RuleId), sqlObj, stmt));
                }
            }

            return problems;
        }
    }
}