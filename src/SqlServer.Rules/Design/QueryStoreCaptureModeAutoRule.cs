using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Design
{
    /// <summary>
    /// Please review the Query Store capture mode database option
    /// </summary>
    /// <FriendlyName>Query Store capture mode is not AUTO</FriendlyName>
    /// <IsIgnorable>false</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// Query Store capture mode should be set to AUTO for supported SQL Server project targets.
    /// </remarks>
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Model)]
    public sealed class QueryStoreCaptureModeAutoRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0702";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Set <QueryStoreCaptureMode>Auto</QueryStoreCaptureMode> in the project file.";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryStoreCaptureModeAutoRule"/> class.
        /// </summary>
        public QueryStoreCaptureModeAutoRule()
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

            if (sqlModel == null || !SupportsQueryStoreCaptureMode(sqlModel.Version))
            {
                return problems;
            }

            var dbOptions = sqlModel.CopyModelOptions();

            if (dbOptions.QueryStoreCaptureMode != QueryStoreCaptureMode.Auto)
            {
                var options = sqlModel.GetObjects(DacQueryScopes.All, ModelSchema.DatabaseOptions).First();
                problems.Add(new SqlRuleProblem(MessageFormatter.FormatMessage(Message, RuleId), options));
            }

            return problems;
        }

        private static bool SupportsQueryStoreCaptureMode(SqlServerVersion version)
        {
            return version is SqlServerVersion.Sql130
                or SqlServerVersion.Sql140
                or SqlServerVersion.Sql150
                or SqlServerVersion.Sql160
                or SqlServerVersion.Sql170;
        }
    }
}
