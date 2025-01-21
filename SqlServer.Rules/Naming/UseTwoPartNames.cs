/*
 * TIM C: 1/19/2018 Commented this out AS the dacpac ALWAYS reports two part names even when the file is missing the schema.
 */

using System.Collections.Generic;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using SqlServer.Dac;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Naming
{
    /// <summary>
    /// Using two part naming on objects [Schema].[Name] is recommended
    /// </summary>
    /// <FriendlyName>Use of default schema</FriendlyName>
    /// <IsIgnorable>false</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// Without specifying the schema in the CREATE script will cause SQL Server to try to assign
    /// the correct schema which will default to the current users default schema and may or may
    /// not be dbo.
    /// </remarks>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Naming,
        RuleScope = SqlRuleScope.Element)]
    public sealed class UseTwoPartNames : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRN0006";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Two part naming on objects is required.";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="UseTwoPartNames"/> class.
        /// </summary>
        public UseTwoPartNames()
            : base(
            ModelSchema.Table,
            ModelSchema.View,
            ModelSchema.Procedure,
            ModelSchema.ScalarFunction,
            ModelSchema.TableValuedFunction,
            ModelSchema.DmlTrigger,
            ModelSchema.Sequence,
            ModelSchema.Default,
            ModelSchema.UserDefinedType,
            ModelSchema.TableType
        )
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

            var fragment = ruleExecutionContext.GetFragment();
            var objectId = fragment.GetObjectName(null);

            if (objectId != null && objectId.Parts.Count < 2)
            {
                problems.Add(new SqlRuleProblem(Message, sqlObj));
            }

            return problems;
        }
    }
}