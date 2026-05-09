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
    /// NUMERIC_ROUNDABORT should be OFF - Required for indexed views.
    /// </summary>
    /// <FriendlyName>NUMERIC_ROUNDABORT should be OFF</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class NumericRoundAbortOffRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0088";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "NUMERIC_ROUNDABORT should be OFF - Required for indexed views.";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="NumericRoundAbortOffRule"/> class.
        /// </summary>
        public NumericRoundAbortOffRule()
            : base(ModelSchema.Procedure, ModelSchema.ScalarFunction, ModelSchema.TableValuedFunction, ModelSchema.DmlTrigger)
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

            var fragment = ruleExecutionContext.ScriptFragment?.GetFragment(
                typeof(CreateProcedureStatement),
                typeof(CreateFunctionStatement),
                typeof(CreateTriggerStatement));
            if (fragment == null)
            {
                return problems;
            }

            var visitor = new PredicateVisitor();
            fragment.Accept(visitor);

            var predicates = from o in visitor.NotIgnoredStatements(RuleId)
                             where o.Options == SetOptions.NumericRoundAbort
                                 && o.IsOn
                             select o;

            problems.AddRange(predicates.Select(predicate =>
                new SqlRuleProblem(MessageFormatter.FormatMessage(Message, RuleId), sqlObj, predicate)));

            return problems;
        }
    }
}
