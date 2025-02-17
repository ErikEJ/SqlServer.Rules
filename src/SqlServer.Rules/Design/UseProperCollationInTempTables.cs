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
    /// Temp table not configured with <c>COLLATION</c> options
    /// </summary>
    /// <FriendlyName>Implicit collation</FriendlyName>
    /// <IsIgnorable>false</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// Create SQL Server temporary tables with the correct collation or use database default as the
    /// tempdb having a different collation than the database can cause issues and or data instability.
    /// </remarks>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public class UseProperCollationInTempTables : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0062";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Create SQL Server temporary tables with the correct collation or use database default as the tempdb having a different collation than the database can cause issues and or data instability.";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="UseProperCollationInTempTables"/> class.
        /// </summary>
        public UseProperCollationInTempTables()
            : base(ModelSchema.Procedure)
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

            if (sqlObj == null)
            {
                return problems;
            }

            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(typeof(CreateProcedureStatement));

            var createTableVisitor = new CreateTableVisitor();
            fragment.Accept(createTableVisitor);

            var statements = createTableVisitor
                .Statements
                .Where(p => p.SchemaObjectName
                            .Identifiers
                            .Any(a => a.Value.StartsWith('#')));

            foreach (var statement in statements)
            {
                var noCollationColumns = statement.Definition.ColumnDefinitions.Where(p => p.Collation == null &&
                            (((SqlDataTypeReference)p.DataType).SqlDataTypeOption == SqlDataTypeOption.VarChar
                                || ((SqlDataTypeReference)p.DataType).SqlDataTypeOption == SqlDataTypeOption.Char
                                || ((SqlDataTypeReference)p.DataType).SqlDataTypeOption == SqlDataTypeOption.NVarChar
                                || ((SqlDataTypeReference)p.DataType).SqlDataTypeOption == SqlDataTypeOption.NChar));
                problems.AddRange(noCollationColumns.Select(s => new SqlRuleProblem(MessageFormatter.FormatMessage(Message, RuleId), sqlObj, s)));
            }

            return problems;
        }
    }
}