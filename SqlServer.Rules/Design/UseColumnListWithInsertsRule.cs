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
    /// Always use a column list in INSERT statements.
    /// </summary>
    /// <FriendlyName>Implicit column list</FriendlyName>
    /// <IsIgnorable>false</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// When inserting into a table or view it is recommended that the target column list be
    /// explicitly specified. This results in more maintainable code and helps in avoiding problems
    /// when the table structure changes (like adding or dropping a column).
    /// </remarks>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class UseColumnListWithInsertsRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0015";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Always use a column list in INSERT statements.";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = "Always use a column list in INSERT statements.";

        /// <summary>
        /// Initializes a new instance of the <see cref="UseColumnListWithInsertsRule"/> class.
        /// </summary>
        public UseColumnListWithInsertsRule() : base(ModelSchema.Procedure)
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
            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(typeof(CreateProcedureStatement));
            var name = sqlObj.Name.GetName();

            var visitor = new InsertStatementVisitor();
            fragment.Accept(visitor);
            if (visitor.Count == 0) { return problems; }

            var offenders = visitor.Statements.Where(s => s.InsertSpecification.Columns.Count == 0).ToList();

            problems.AddRange(offenders.Select(o => new SqlRuleProblem(Message, sqlObj, o)));

            return problems;
        }
    }
}