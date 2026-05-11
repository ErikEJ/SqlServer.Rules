using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Design
{
    /// <summary>
    /// Please review the TARGET_RECOVERY_TIME database option
    /// </summary>
    /// <FriendlyName>Database TARGET_RECOVERY_TIME option is not set</FriendlyName>
    /// <IsIgnorable>false</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// The database TARGET_RECOVERY_TIME option should be set to a value greater than 0.
    /// Setting TARGET_RECOVERY_TIME to a positive value enables indirect checkpoints and can improve recovery time.
    /// </remarks>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Model)]
    public sealed class TargetRecoveryTimePeriodRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0704";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Set <TargetRecoveryTimePeriod>60</TargetRecoveryTimePeriod> in the project file to enable TARGET_RECOVERY_TIME.";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="TargetRecoveryTimePeriodRule"/> class.
        /// </summary>
        public TargetRecoveryTimePeriodRule()
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

            if (dbOptions.TargetRecoveryTimePeriod.GetValueOrDefault(0) <= 0)
            {
                var options = sqlModel.GetObjects(DacQueryScopes.All, ModelSchema.DatabaseOptions).First();
                problems.Add(new SqlRuleProblem(MessageFormatter.FormatMessage(Message, RuleId), options));
            }

            return problems;
        }
    }
}
