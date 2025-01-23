using System.Collections.Generic;
using System.Globalization;
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
    /// Avoid using columns that match other columns by name, but are different in type or size
    /// </summary>
    /// <FriendlyName>Ambiguous column name across design</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// Columns are found in multiple tables that match by name but differ by either type or size.
    /// If the columns truly have different meanings, they should differ by name as well or they
    /// should match in datatype and size.
    /// </remarks>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Model)]
    public sealed class ConsiderMatchingColumnDataTypes : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0047";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Avoid using columns that match other columns by name, but are different in type or size.";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = "Avoid using columns ({0}) that match other columns by name in the database, but are different in type or size.";

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsiderMatchingColumnDataTypes"/> class.
        /// </summary>
        public ConsiderMatchingColumnDataTypes()
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

            var tables = sqlModel.GetObjects(DacQueryScopes.UserDefined, Table.TypeClass).Where(t => !t.IsWhiteListed());
            var columnList = new List<TableColumnInfo>();

            foreach (var table in tables)
            {
                var fragment = table.GetFragment();
                var columnVisitor = new ColumnDefinitionVisitor();
                fragment.Accept(columnVisitor);
                columnList.AddRange(columnVisitor.NotIgnoredStatements(RuleId)
                    .Where(col => col.DataType != null)
                    .Select(col =>
                    new TableColumnInfo
                    {
                        TableName = table.Name.GetName(),
                        ColumnName = col.ColumnIdentifier.Value,
                        DataType = col.DataType.Name.Identifiers.FirstOrDefault()?.Value,
                        DataTypeParameters = GetDataTypeLengthParameters(col),
                        Column = col,
                        Table = table,
                    }));
            }

            // find all the columns that match by name but differ by data type or length....
            var offenders = columnList.Where(x =>
                columnList.Any(y =>
                    !Comparer.Equals(x.TableName, y.TableName)
                    && Comparer.Equals(x.ColumnName, y.ColumnName)
                    && (
                        !Comparer.Equals(x.DataType, y.DataType)
                        || !Comparer.Equals(x.DataTypeParameters, y.DataTypeParameters)
                    )));

            problems.AddRange(offenders
                .Select(col => new SqlRuleProblem(MessageFormatter.FormatMessage(string.Format(CultureInfo.InvariantCulture, Message, col), RuleId), col.Table, col.Column)));

            return problems;
        }

        internal static string GetDataTypeLengthParameters(ColumnDefinition col)
        {
            if (col.DataType is SqlDataTypeReference dataType)
            {
                return string.Join(",", dataType.GetDataTypeParameters());
            }

            return string.Empty;
        }

        private sealed class TableColumnInfo
        {
            public string TableName { get; set; }

            public string ColumnName { get; set; }

            public string DataType { get; set; }

            public string DataTypeParameters { get; set; }

            public ColumnDefinition Column { get; set; }

            public TSqlObject Table { get; set; }

            public override string ToString()
            {
                return $"{ColumnName} {DataType}({DataTypeParameters.Replace("-1", "MAX", System.StringComparison.OrdinalIgnoreCase)})";
            }
        }
    }
}