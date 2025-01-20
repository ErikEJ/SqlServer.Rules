using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using SqlServer.Dac;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Design
{
    /// <summary>
    /// Avoid creating very wide primary keys with guids or (n)varchar as the first column in the index.
    /// </summary>
    /// <FriendlyName>Avoid wide primary keys</FriendlyName>
    /// <IsIgnorable>false</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class WidePKsRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0003";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Primary Keys should avoid using GUIDS or wide VARCHAR columns.";

        /// <summary>
        /// The unique identifier message
        /// </summary>
        private const string GuidMessage = "Guids should not be used as the first column in a primary key.";

        /// <summary>
        /// The wide varchar message
        /// </summary>
        private const string WideVarcharMessage = "Wide (n)varchar columns should not be used in primary keys.";

        /// <summary>
        /// Initializes a new instance of the <see cref="WidePKsRule"/> class.
        /// </summary>
        public WidePKsRule()
            : base(ModelSchema.PrimaryKeyConstraint)
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

            var objName = sqlObj.Name.GetName();

            var columns = sqlObj.GetReferenced(DacQueryScopes.All).Where(x => x.ObjectType == Column.TypeClass).ToList();
            if (columns.Count == 0)
            {
                return problems;
            }

            var keyColumn = columns.FirstOrDefault();
            var dataType = keyColumn.GetReferenced(Column.DataType, DacQueryScopes.All).FirstOrDefault();
            if (dataType?.Name == null)
            {
                return problems;
            }

            var dataTypeName = dataType.Name.Parts.Last();
            if (Comparer.Equals(dataTypeName, "uniqueidentifier"))
            {
                problems.Add(new SqlRuleProblem(GuidMessage, sqlObj));
            }

            if (columns.Any(col =>
            {
                var len = col.GetProperty<int>(Column.Length);
                dataTypeName = col.GetReferenced(Column.DataType).First().Name.Parts.Last();
                return (Comparer.Equals(dataTypeName, "varchar") && len > 50)
                    || (Comparer.Equals(dataTypeName, "nvarchar") && len > 100);
            }))
            {
                problems.Add(new SqlRuleProblem(WideVarcharMessage, sqlObj));
            }

            return problems;
        }
    }
}