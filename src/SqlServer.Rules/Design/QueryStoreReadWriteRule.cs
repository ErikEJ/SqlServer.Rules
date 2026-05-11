using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Design
{
    /// <summary>
    /// Please review the QUERY_STORE database option
    /// </summary>
    /// <FriendlyName>Database QUERY_STORE option is not READ_WRITE</FriendlyName>
    /// <IsIgnorable>false</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// The database QUERY_STORE option should be set to READ_WRITE.
    /// </remarks>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Model)]
    public sealed class QueryStoreReadWriteRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0701";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Set <QueryStoreDesiredState>READ_WRITE</QueryStoreDesiredState> in the project file to enable Query Store.";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryStoreReadWriteRule"/> class.
        /// </summary>
        public QueryStoreReadWriteRule()
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

            if (sqlModel == null || !IsQueryStoreSupported(sqlModel.Version))
            {
                return problems;
            }

            var dbOptions = sqlModel.CopyModelOptions();

            if (dbOptions.QueryStoreDesiredState != QueryStoreDesiredState.ReadWrite)
            {
                var options = sqlModel.GetObjects(DacQueryScopes.All, ModelSchema.DatabaseOptions).FirstOrDefault();
                if (options != null)
                {
                    problems.Add(new SqlRuleProblem(MessageFormatter.FormatMessage(Message, RuleId), options));
                }
            }

            return problems;
        }

        private static bool IsQueryStoreSupported(SqlServerVersion version)
            => version != SqlServerVersion.SqlAzure
            && version is not SqlServerVersion.Sql90
            and not SqlServerVersion.Sql100
            and not SqlServerVersion.Sql110
            and not SqlServerVersion.Sql120;
    }
}
