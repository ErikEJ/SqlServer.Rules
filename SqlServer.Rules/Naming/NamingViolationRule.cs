using System;
using System.Collections.Generic;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using SqlServer.Dac;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Naming
{
    /// <summary>
    ///  Base class for naming validations
    /// </summary>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    public class NamingViolationRule : BaseSqlCodeAnalysisRule
    {
        private readonly string ruleId;

        /// <summary>
        /// The message
        /// </summary>
        protected string Message { get; }

        /// <summary>
        /// The bad characters
        /// </summary>
        protected string BadCharacters { get; }

        /// <summary>
        /// The partial predicate
        /// </summary>
        protected Func<string, Predicate<string>> PartialPredicate { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NamingViolationRule"/> class.
        /// </summary>
        /// <param name="ruleId">The rule identifier.</param>
        /// <param name="message">The message.</param>
        /// <param name="badPrefix">The bad prefix.</param>
        /// <param name="appliesTo">The applies to.</param>
        /// <param name="predicate">The predicate.</param>
        public NamingViolationRule(
            string ruleId,
            string message,
            string badPrefix,
            IList<ModelTypeClass> appliesTo,
            Func<string, Predicate<string>> predicate)
        {
            this.ruleId = ruleId;
            Message = message;
            BadCharacters = badPrefix;
            SupportedElementTypes = appliesTo;
            PartialPredicate = predicate;
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

            var name = ruleExecutionContext.GetObjectName(sqlObj, ElementNameStyle.SimpleName).ToUpperInvariant();
            var fragment = ruleExecutionContext.GetFragment();

            if (PartialPredicate(name)(BadCharacters)
                && Ignorables.ShouldNotIgnoreRule(fragment.ScriptTokenStream, ruleId, fragment.StartLine))
            {
                problems.Add(new SqlRuleProblem(Message, sqlObj));
            }

            return problems;
        }
    }
}