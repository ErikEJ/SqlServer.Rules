using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Design
{
    /// <summary>
    /// Avoid the use of long (N)CHAR types in tables. Use (N)VARCHAR instead.
    /// </summary>
    /// <FriendlyName>Avoid long CHAR types</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class AvoidCHARTypeRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0005";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Avoid the (n)char column type except for short static length data.";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = "Avoid the (n)char column type except for short static length data.";

        /// <summary>
        /// Initializes a new instance of the <see cref="AvoidCHARTypeRule"/> class.
        /// </summary>
        public AvoidCHARTypeRule()
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

            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(typeof(CreateTableStatement));
            var tableName = sqlObj.Name.GetName();

            var columnVisitor = new ColumnDefinitionVisitor();
            fragment.Accept(columnVisitor);

            var longChars = columnVisitor.NotIgnoredStatements(RuleId)
                .Where(col => col.DataType?.Name != null)
                .Select(col => new
                {
                    column = col,
                    name = col.ColumnIdentifier.Value,
                    type = col.DataType.Name.Identifiers.FirstOrDefault()?.Value,
                    length = GetDataTypeLength(col),
                })
                .Where(x => (Comparer.Equals(x.type, "char") || Comparer.Equals(x.type, "nchar")) && x.length > 19);

            problems.AddRange(longChars.Select(col => new SqlRuleProblem(MessageFormatter.FormatMessage(Message, RuleId), sqlObj, col.column)));

            return problems;
        }

        private static decimal GetDataTypeLength(ColumnDefinition col)
        {
            if (col.DataType is SqlDataTypeReference dataType)
            {
                return dataType.GetDataTypeParameters().FirstOrDefault();
            }

            return 0;
        }
    }
}