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
    /// Object has different collation than the rest of the database.
    /// </summary>
    /// <FriendlyName>Explicit collation other</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class AvoidObjectUsesDifferentCollationRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0053";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Object has different collation than the rest of the database. Try to avoid using a different collation unless by design.";

        /// <summary>
        /// The message column
        /// </summary>
        public const string MessageColumn = "This column has a different collation than the rest of the database. Try to avoid using a different collation unless by design.";

        /// <summary>
        /// The message default
        /// </summary>
        public const string MessageDefault = "This default constraint has a different collation than the rest of the database. Try to avoid using a different collation unless by design.";

        /// <summary>
        /// Initializes a new instance of the <see cref="AvoidObjectUsesDifferentCollationRule"/> class.
        /// </summary>
        public AvoidObjectUsesDifferentCollationRule()
            : base(ModelSchema.Table)
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

            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(typeof(CreateTableStatement));
            var objName = sqlObj.Name.GetName();

            var dbCollation = ruleExecutionContext.SchemaModel.CopyModelOptions().Collation;

            var columnVisitor = new ColumnDefinitionVisitor();
            fragment.Accept(columnVisitor);

            var statements = columnVisitor.NotIgnoredStatements(RuleId).ToList();

            var columnOffenders = statements.Where(col =>
                (col.Collation != null && !Comparer.Equals(col.Collation?.Value, dbCollation))).ToList();

            problems.AddRange(columnOffenders.Select(col => new SqlRuleProblem(MessageColumn, sqlObj, col)));

            var defaultOffenders = statements.Where(col =>
            {
                var collation = (col.DefaultConstraint?.Expression as PrimaryExpression)?.Collation;

                return collation != null && !Comparer.Equals(collation.Value, dbCollation);
            }).ToList();

            problems.AddRange(defaultOffenders.Select(col => new SqlRuleProblem(MessageDefault, sqlObj, col.DefaultConstraint)));

            return problems;
        }
    }
}