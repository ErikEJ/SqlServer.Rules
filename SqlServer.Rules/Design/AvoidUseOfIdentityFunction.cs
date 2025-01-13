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
    /// <summary>Use OUTPUT or SCOPE_IDENTITY() instead of @@IDENTITY</summary>
    /// <FriendlyName>Unsafe identity retrieval</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// The rule checks the code for using any of the `@@IDENTITY` function. When the queries use
    /// parallel execution plans, the identity functions may return incorrect results.
    /// </remarks>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public class AvoidUseOfIdentityFunction : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0056";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Use OUTPUT or SCOPE_IDENTITY() instead of @@IDENTITY.";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="AvoidUseOfIdentityFunction"/> class.
        /// </summary>
        public AvoidUseOfIdentityFunction() : base(ModelSchema.Procedure, ModelSchema.ScalarFunction, ModelSchema.TableValuedFunction, ModelSchema.DmlTrigger)
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

            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(typeof(CreateProcedureStatement), typeof(CreateFunctionStatement), typeof(CreateTriggerStatement));

            var visitor = new GlobalVariableExpressionVisitor("@@identity");

            fragment.Accept(visitor);

            problems.AddRange(visitor.NotIgnoredStatements(RuleId).Select(s => new SqlRuleProblem(Message, sqlObj, s)));

            return problems;
        }
    }
}