using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Design
{
    /// <summary>
    /// Please review the AUTO_SHRINK database option
    /// </summary>
    /// <FriendlyName>Database AUTO_SHRINK option is ON</FriendlyName>
    /// <IsIgnorable>false</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// The database AUTO_SHRINK option should be OFF.
    /// </remarks>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Model)]
    public sealed class AutoShrinkOffRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0706";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Set <AutoShrink>False</AutoShrink> in the project file to disable AUTO_SHRINK.";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

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
            var sqlModel = ruleExecutionContext.SchemaModel;

            if (sqlModel == null)
            {
                return problems;
            }

            if (sqlModel.Version == SqlServerVersion.SqlAzure)
            {
                return problems;
            }

            var dbOptions = sqlModel.CopyModelOptions();

            if (dbOptions.AutoShrink.GetValueOrDefault(false))
            {
                var options = sqlModel.GetObjects(DacQueryScopes.All, ModelSchema.DatabaseOptions).First();
                problems.Add(new SqlRuleProblem(MessageFormatter.FormatMessage(Message, RuleId), options));
            }

            return problems;
        }
    }
}
