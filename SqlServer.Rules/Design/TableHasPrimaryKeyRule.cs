using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Design
{
    /// <summary>
    /// Tables should have a primary key
    /// </summary>
    /// <FriendlyName>Missing primary key</FriendlyName>
    /// <IsIgnorable>false</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class TableHasPrimaryKeyRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0002";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Table does not have a primary key.";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="TableHasPrimaryKeyRule"/> class.
        /// </summary>
        public TableHasPrimaryKeyRule()
            : base(ModelSchema.Table)
        {
            SupportedElementTypes = new[] { ModelSchema.Table };
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

            var child = sqlObj.GetChildren(DacQueryScopes.All)
                .FirstOrDefault(x => x.ObjectType == ModelSchema.PrimaryKeyConstraint);
            if (child == null)
            {
                var parentObj = sqlObj.Name.HasName ? sqlObj : sqlObj.GetParent(DacQueryScopes.All);

                problems.Add(new SqlRuleProblem(Message, parentObj));
            }

            return problems;
        }
    }
}