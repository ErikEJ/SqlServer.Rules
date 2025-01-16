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
    /// 
    /// </summary>
    /// <FriendlyName></FriendlyName>
    /// <IsIgnorable>false</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class DoNotUseDeprecatedTypesRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0051";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Don't use deprecated TEXT, NTEXT and IMAGE data types.";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="DoNotUseDeprecatedTypesRule"/> class.
        /// </summary>
        public DoNotUseDeprecatedTypesRule() : base(ModelSchema.Table)
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

            if (sqlObj == null || sqlObj.IsWhiteListed()) { return problems; }
            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(typeof(CreateTableStatement));
            var objName = sqlObj.Name.GetName();

            var columnVisitor = new ColumnDefinitionVisitor();
            fragment.Accept(columnVisitor);

            var offenders = columnVisitor.Statements
                .Where(col => col.DataType?.Name != null)
                .Select(col => new {
                    column = col,
                    name = col.ColumnIdentifier.Value,
                    type = col.DataType.Name.Identifiers.FirstOrDefault()?.Value,
                })
                .Where(x => Comparer.Equals(x.type, "text")
                    || Comparer.Equals(x.type, "ntext")
                    || Comparer.Equals(x.type, "image"));

            problems.AddRange(offenders.Select(col => new SqlRuleProblem(Message, sqlObj, col.column)));

            return problems;
        }
    }
}