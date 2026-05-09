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
    /// Check constraints on temp tables should not be named.
    /// </summary>
    /// <FriendlyName>Named check constraints on temp tables</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// Naming check constraints on temporary tables can cause contention issues in tempdb.
    /// </remarks>
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class AvoidNamedCheckOnTempTableRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier.
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0095";

        /// <summary>
        /// The rule display name.
        /// </summary>
        public const string RuleDisplayName = "Check constraints on temp tables should not be named.";

        /// <summary>
        /// The message.
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="AvoidNamedCheckOnTempTableRule"/> class.
        /// </summary>
        public AvoidNamedCheckOnTempTableRule()
            : base(ProgrammingSchemas)
        {
        }

        /// <summary>
        /// Performs analysis and returns a list of problems detected.
        /// </summary>
        /// <param name="ruleExecutionContext">Contains the schema model and model element to analyze.</param>
        /// <returns>The problems detected by the rule in the given element.</returns>
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

            var createTableVisitor = new CreateTableVisitor
            {
                TypeFilter = ObjectTypeFilter.TempOnly,
            };
            fragment.Accept(createTableVisitor);

            var tempTableStatements = createTableVisitor.Statements
                .Where(statement => statement.SchemaObjectName?.BaseIdentifier?.Value.StartsWith("#", System.StringComparison.Ordinal) == true);

            foreach (var statement in tempTableStatements)
            {
                var tableConstraints = statement.Definition.TableConstraints
                    .OfType<CheckConstraintDefinition>();

                var columnConstraints = statement.Definition.ColumnDefinitions
                    .SelectMany(c => c.Constraints)
                    .OfType<CheckConstraintDefinition>();

                var offenders = tableConstraints
                    .Concat(columnConstraints)
                    .Where(c => c.ConstraintIdentifier != null)
                    .ToList();

                problems.AddRange(offenders.Select(o =>
                    new SqlRuleProblem(MessageFormatter.FormatMessage(Message, RuleId), sqlObj, o)));
            }

            return problems;
        }
    }
}
