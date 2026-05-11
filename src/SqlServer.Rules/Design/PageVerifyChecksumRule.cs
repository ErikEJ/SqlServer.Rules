using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Design
{
    /// <summary>
    /// Please review the PAGE_VERIFY database option
    /// </summary>
    /// <FriendlyName>Database PAGE_VERIFY option is not CHECKSUM</FriendlyName>
    /// <IsIgnorable>false</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// The database PAGE_VERIFY option should be set to CHECKSUM.
    /// CHECKSUM detects more errors than TORN_PAGE_DETECTION or NONE, and is the recommended setting.
    /// </remarks>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Model)]
    public sealed class PageVerifyChecksumRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0700";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Set `<PageVerify>CHECKSUM</PageVerify>` in the project file to enable PAGE_VERIFY = CHECKSUM.";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="PageVerifyChecksumRule"/> class.
        /// </summary>
        public PageVerifyChecksumRule()
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

            if (dbOptions.PageVerifyMode != PageVerifyMode.Checksum)
            {
                var options = sqlModel.GetObjects(DacQueryScopes.All, ModelSchema.DatabaseOptions).First();
                problems.Add(new SqlRuleProblem(MessageFormatter.FormatMessage(Message, RuleId), options));
            }

            return problems;
        }
    }
}
