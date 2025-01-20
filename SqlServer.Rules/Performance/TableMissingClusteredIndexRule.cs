using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using SqlServer.Rules.Globals;
using Index = Microsoft.SqlServer.Dac.Model.Index;

namespace SqlServer.Rules.Design
{
    /// <summary>Consider adding clustered index to table.</summary>
    /// <FriendlyName>Missing Clustered index</FriendlyName>
    /// <IsIgnorable>false</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>Tables that do not have clustered index should be the exception not the rule.</remarks>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Performance,
        RuleScope = SqlRuleScope.Element)]
    public sealed class TableMissingClusteredIndexRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRP0020";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Table does not have a clustered index.";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="TableMissingClusteredIndexRule"/> class.
        /// </summary>
        public TableMissingClusteredIndexRule()
            : base(ModelSchema.Table)
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

            var indexes = sqlObj.GetChildren(DacQueryScopes.All)
                .Where(x =>
                    x.ObjectType == ModelSchema.Index
                    || x.ObjectType == ModelSchema.UniqueConstraint
                    || x.ObjectType == ModelSchema.PrimaryKeyConstraint).ToList();
            if (!indexes.Any(i => IsClustered(i)))
            {
                problems.Add(new SqlRuleProblem(Message, sqlObj));
            }

            return problems;
        }

        private static bool IsClustered(TSqlObject i)
        {
            if (i.ObjectType == ModelSchema.Index)
            {
                return Convert.ToBoolean(i.GetProperty(Index.Clustered), CultureInfo.InvariantCulture);
            }

            if (i.ObjectType == ModelSchema.UniqueConstraint)
            {
                return Convert.ToBoolean(i.GetProperty(UniqueConstraint.Clustered), CultureInfo.InvariantCulture);
            }

            return Convert.ToBoolean(i.GetProperty(PrimaryKeyConstraint.Clustered), CultureInfo.InvariantCulture);
        }
    }
}