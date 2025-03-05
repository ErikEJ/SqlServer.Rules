using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Design
{
    /// <summary>
    /// Avoid future keywords.
    /// </summary>
    /// <FriendlyName>Avoid the use of future keywords as identifiers.</FriendlyName>
    /// <IsIgnorable>false</IsIgnorable>
    /// <ExampleMd>
    /// Examples of **incorrect** code for this rule:
    /// ```tsql
    /// CREATE TABLE bad (absolute int);
    /// ```
    /// Examples of ** correct** code for this rule:
    /// ```tsql
    /// CREATE TABLE good (absalute int);
    /// ```
    /// </ExampleMd>
    /// <remarks>
    /// Future keywords could be reserved in future releases of SQL Server as new features are implemented. Consider avoiding the use of these words as identifiers.
    /// </remarks>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class AvoidFutureKeywordsRule : BaseSqlCodeAnalysisRule
    {
        private readonly HashSet<string> sqlWords;

        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0069";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Avoid the use of future keywords as identifiers.";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = "Avoid using the future keyword '{0}'.";

        /// <summary>
        /// Initializes a new instance of the <see cref="AvoidFutureKeywordsRule"/> class.
        /// </summary>
        public AvoidFutureKeywordsRule()
            : base(ModelSchema.Procedure, ModelSchema.Table, ModelSchema.View, ModelSchema.ScalarFunction, ModelSchema.TableValuedFunction)
        {
            sqlWords = new HashSet<string>(TSqlFutureKeywords, StringComparer.OrdinalIgnoreCase);
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

            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(typeof(CreateProcedureStatement), typeof(CreateFunctionStatement), typeof(CreateTriggerStatement), typeof(CreateTableStatement), typeof(CreateViewStatement));

            if (fragment == null)
            {
                return problems;
            }

            for (var index = 0; index < fragment.ScriptTokenStream?.Count; index++)
            {
                var token = fragment.ScriptTokenStream[index];

                if (token is null)
                {
                    continue;
                }

                if (token.TokenType != TSqlTokenType.Identifier)
                {
                    continue;
                }

                if (sqlWords.Contains(token.Text))
                {
                    problems.Add(new SqlRuleProblem(MessageFormatter.FormatMessage(string.Format(CultureInfo.InvariantCulture, Message, token.Text), RuleId), sqlObj, fragment));
                }
            }

            return problems;
        }
    }
}