using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Design
{
    /// <summary>
    /// Please review ANSI, arithmetic, concat, and identifier options
    /// </summary>
    /// <FriendlyName>Invalid database configured options</FriendlyName>
    /// <IsIgnorable>false</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// The database is configured with invalid options. 
    /// Many of these options can have an adverse affect as well as affect performance.
    /// <br/>
    /// Recommended: 
    /// <list type="table">
    ///   <listheader><term>Option</term><description>State</description></listheader>
    ///   <item><term>ANSI_NULLS</term><description>OFF</description></item>
    ///   <item><term>ANSI_PADDING</term><description>OFF</description></item>
    ///   <item><term>ANSI_WARNINGS</term><description>OFF</description></item>
    ///   <item><term>ARITHABORT</term><description>OFF</description></item>
    ///   <item><term>CONCAT_NULL_YIELDS_NULL</term><description>OFF</description></item>
    ///   <item><term>QUOTED_IDENTIFIER</term><description>OFF</description></item>
    /// </list>
    /// </remarks>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Model)]
    public sealed class InvalidDatabaseOptionsRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0061";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "The database is configured with invalid options.";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = "The database is configured with these invalid options: {0}.";

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidDatabaseOptionsRule"/> class.
        /// </summary>
        public InvalidDatabaseOptionsRule()
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

            var dbOptions = sqlModel.CopyModelOptions();
            var invalidOptions = new List<string>();

            if (!dbOptions.AnsiNullsOn.GetValueOrDefault(true)) { invalidOptions.Add("ANSI_NULLS OFF"); }
            if (!dbOptions.AnsiPaddingOn.GetValueOrDefault(true)) { invalidOptions.Add("ANSI_PADDING OFF"); }
            if (!dbOptions.AnsiWarningsOn.GetValueOrDefault(true)) { invalidOptions.Add("ANSI_WARNINGS OFF"); }
            if (!dbOptions.ArithAbortOn.GetValueOrDefault(true)) { invalidOptions.Add("ARITHABORT OFF"); }
            if (!dbOptions.ConcatNullYieldsNull.GetValueOrDefault(true)) { invalidOptions.Add("CONCAT_NULL_YIELDS_NULL OFF"); }
            if (!dbOptions.QuotedIdentifierOn.GetValueOrDefault(true)) { invalidOptions.Add("QUOTED_IDENTIFIER OFF"); }

            if (invalidOptions.Count > 0)
            {
                // cant pass null into the SqlRuleProblem ctor so we have to create or get a tsqlobject
                var options = sqlModel.GetObjects(DacQueryScopes.All, ModelSchema.DatabaseOptions).First();
                problems.Add(new SqlRuleProblem(string.Format(CultureInfo.InvariantCulture, Message, string.Join(", ", invalidOptions)), options));
            }

            return problems;
        }
    }
}