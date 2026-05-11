using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Design
{
    /// <summary>
    /// Please review the QUERY_STORE_CAPTURE_MODE database option
    /// </summary>
    /// <FriendlyName>Database QUERY_STORE_CAPTURE_MODE option is not AUTO</FriendlyName>
    /// <IsIgnorable>false</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// The database QUERY_STORE_CAPTURE_MODE option should be set to AUTO.
    /// AUTO captures relevant queries based on execution count and resource consumption.
    /// </remarks>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
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
        public const string RuleId = Constants.RuleNameSpace + "SRD0703";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Set <QueryStoreCaptureMode>AUTO</QueryStoreCaptureMode> in the project file to enable QUERY_STORE_CAPTURE_MODE = AUTO.";

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

            if (sqlModel == null)
            {
                return problems;
            }

            if (sqlModel.Version == SqlServerVersion.SqlAzure || sqlModel.Version < SqlServerVersion.Sql130)
            {
                return problems;
            }

            var options = sqlModel.GetObjects(DacQueryScopes.All, ModelSchema.DatabaseOptions).FirstOrDefault();
            if (options == null)
            {
                return problems;
            }

            var dbOptions = sqlModel.CopyModelOptions();

            if (dbOptions.QueryStoreCaptureMode != QueryStoreCaptureMode.Auto)
            {
                problems.Add(new SqlRuleProblem(MessageFormatter.FormatMessage(Message, RuleId), options));
            }

            return problems;
        }
    }
}
