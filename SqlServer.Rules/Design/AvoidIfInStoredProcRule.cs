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
    /// Avoid wrapping SQL statements in stored procedures with IF statements. Consider extracting the nested SQL statements to their own stored procedure.
    /// </summary>
    /// <FriendlyName>Avoid wrapping SQL in IF statement</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class AvoidIfInStoredProcRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0063";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Do not use IF statements containing queries in stored procedures.";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="AvoidIfInStoredProcRule"/> class.
        /// </summary>
        public AvoidIfInStoredProcRule() : base(ModelSchema.Procedure)
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

            if (sqlObj == null) { return problems; }
            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(typeof(CreateProcedureStatement));

            var ifVisitor = new IfStatementVisitor();

            fragment.Accept(ifVisitor);

            if (!ifVisitor.Statements.Any()) { return problems; }

            foreach (var ifStatement in ifVisitor.NotIgnoredStatements(RuleId))
            {
                var tableVisitor = new NamedTableReferenceVisitor { TypeFilter = ObjectTypeFilter.PermanentOnly };
                ifStatement.ThenStatement?.Accept(tableVisitor);
                ifStatement.ElseStatement?.Accept(tableVisitor);
                problems.AddRange(tableVisitor.Statements.Select(s => new SqlRuleProblem(Message, sqlObj, s)));
            }

            return problems;
        }
    }
}