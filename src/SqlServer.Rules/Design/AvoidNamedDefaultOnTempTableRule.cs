using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Design
{
    /// <summary>
    /// Default constraints on temporary tables should not be named.
    /// </summary>
    /// <FriendlyName>Do not name default constraints on temporary tables</FriendlyName>
    /// <IsIgnorable>false</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// Naming default constraints on temporary tables can cause tempdb contention.
    /// </remarks>
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class AvoidNamedDefaultOnTempTableRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier.
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0093";

        /// <summary>
        /// The rule display name.
        /// </summary>
        public const string RuleDisplayName = "Default constraints on temporary tables should not be named.";

        /// <summary>
        /// The message.
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="AvoidNamedDefaultOnTempTableRule"/> class.
        /// </summary>
        public AvoidNamedDefaultOnTempTableRule()
            : base(ProgrammingAndViewSchemas)
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

            var fragment = ruleExecutionContext.ScriptFragment?.GetFragment(ProgrammingAndViewSchemaTypes);
            if (fragment == null)
            {
                return problems;
            }

            var createTableVisitor = new CreateTableVisitor { TypeFilter = ObjectTypeFilter.TempOnly };
            fragment.Accept(createTableVisitor);

            var offenders = createTableVisitor.NotIgnoredStatements(RuleId)
                .Where(statement =>
                    statement?.SchemaObjectName?.Identifiers != null &&
                    statement.SchemaObjectName.Identifiers.Any() &&
                    statement.SchemaObjectName.Identifiers.Last().Value.StartsWith("#", System.StringComparison.Ordinal))
                .SelectMany(statement => statement.Definition.ColumnDefinitions)
                .Where(column => column.DefaultConstraint?.ConstraintIdentifier != null);

            problems.AddRange(offenders.Select(column =>
                new SqlRuleProblem(MessageFormatter.FormatMessage(Message, RuleId), sqlObj, column)));

            return problems;
        }
    }
}
