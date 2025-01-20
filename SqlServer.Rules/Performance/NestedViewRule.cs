using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using SqlServer.Dac;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Performance
{
    /// <summary>Views should not use other views as a data source</summary>
    /// <FriendlyName>Nested Views</FriendlyName>
    /// <IsIgnorable>false</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>Views that use other views in their from cause are extremely inefficient and will result in non-optimal execution plans.</remarks>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Performance,
        RuleScope = SqlRuleScope.Element)]
    public sealed class NestedViewRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRP0001";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Views should not use other views as a data source.";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="NestedViewRule"/> class.
        /// </summary>
        public NestedViewRule()
            : base(ModelSchema.View)
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

            var sqlObjName = ruleExecutionContext.GetObjectName(sqlObj);
            foreach (var child in sqlObj.GetReferenced(DacQueryScopes.UserDefined).Where(x => x.ObjectType == ModelSchema.View))
            {
                problems.Add(new SqlRuleProblem(Message, sqlObj) /* { Severity = SqlRuleProblemSeverity.Error } */);
            }

            return problems;
        }
    }
}