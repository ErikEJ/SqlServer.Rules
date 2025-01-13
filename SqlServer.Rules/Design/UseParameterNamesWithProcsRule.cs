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
    /// Always use parameter names when calling stored procedures
    /// </summary>
    /// <FriendlyName>Ordinal parameters used</FriendlyName>
    /// <IsIgnorable>false</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// The rule checks EXECUTE statements for stored procedure not being called with named
    /// parameters, but with parameters by position.
    /// </remarks>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class UseParameterNamesWithProcsRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0058";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Always use parameter names when calling stored procedures.";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="UseParameterNamesWithProcsRule"/> class.
        /// </summary>
        public UseParameterNamesWithProcsRule() : base(ModelSchema.Procedure)
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

            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(typeof(CreateProcedureStatement));

            var execVisitor = new ExecuteVisitor();
            fragment.Accept(execVisitor);

            if (execVisitor.Count == 0) { return problems; }

            foreach (var exec in execVisitor.Statements)
            {
                if (exec.ExecuteSpecification.ExecutableEntity is ExecutableProcedureReference proc
                    && proc.Parameters.Any(p => p.Variable == null))
                {
                    problems.Add(new SqlRuleProblem(Message, sqlObj, exec));
                }
            }

            return problems;
        }
    }
}