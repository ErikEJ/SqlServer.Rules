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
    /// <summary>
    /// Specify whether table columns are nullable.
    /// </summary>
    /// <FriendlyName>Explicit column nullability</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// Column nullability defaults can depend on the ANSI_NULL_DFLT settings. Specify NULL or
    /// NOT NULL explicitly to make the intended column definition clear and consistent.
    /// </remarks>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class ExplicitColumnNullabilityRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0098";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Specify NULL or NOT NULL explicitly for table columns.";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExplicitColumnNullabilityRule"/> class.
        /// </summary>
        public ExplicitColumnNullabilityRule()
            : base(ModelSchema.Table)
        {
        }

        /// <summary>
        /// Performs analysis and returns a list of problems detected
        /// </summary>
        /// <param name="ruleExecutionContext">Contains the schema model and model element to analyze</param>
        /// <returns>The problems detected in the given element</returns>
        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            var problems = new List<SqlRuleProblem>();
            var sqlObj = ruleExecutionContext.ModelElement;

            if (sqlObj == null || sqlObj.IsWhiteListed())
            {
                return problems;
            }

            var fragment = ruleExecutionContext.ScriptFragment?.GetFragment(typeof(CreateTableStatement)) as CreateTableStatement;

            if (fragment == null)
            {
                return problems;
            }

            var columnVisitor = new ColumnDefinitionVisitor();
            fragment.Accept(columnVisitor);

            var primaryKeyColumns = fragment.Definition.TableConstraints
                .OfType<UniqueConstraintDefinition>()
                .Where(constraint => constraint.IsPrimaryKey)
                .SelectMany(constraint => constraint.Columns)
                .Select(column => column.Column.MultiPartIdentifier?.Identifiers.LastOrDefault()?.Value)
                .Where(columnName => columnName != null)
                .ToHashSet(Comparer);

            var columns = columnVisitor.NotIgnoredStatements(RuleId)
                .Where(column => column.ComputedColumnExpression == null)
                .Where(column => column.IdentityOptions == null)
                .Where(column => column.StorageOptions?.SparseOption != SparseColumnOption.Sparse)
                .Where(column => !column.Constraints.OfType<UniqueConstraintDefinition>().Any(constraint => constraint.IsPrimaryKey))
                .Where(column => !primaryKeyColumns.Contains(column.ColumnIdentifier.Value))
                .Where(column => !column.Constraints.Any(constraint => constraint is NullableConstraintDefinition));

            problems.AddRange(columns.Select(column => new SqlRuleProblem(MessageFormatter.FormatMessage(Message, RuleId), sqlObj, column)));

            return problems;
        }
    }
}
