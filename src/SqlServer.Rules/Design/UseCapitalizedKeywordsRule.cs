using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Design
{
    /// <summary>
    /// Use capitalized keywords.
    /// </summary>
    /// <FriendlyName>Use capitalized keywords for enhanced readability.</FriendlyName>
    /// <IsIgnorable>false</IsIgnorable>
    /// <ExampleMd>
    /// Examples of **incorrect** code for this rule:
    /// ```tsql
    /// select * from foo;
    /// ```
    /// Examples of ** correct** code for this rule:
    /// ```tsql
    /// SELECT * FROM foo;
    /// ```
    /// </ExampleMd>
    /// <remarks>
    /// Capitalizing SQL keywords enhances readability and provides a clear separation between keywords and objects.
    /// </remarks>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class UseCapitalizedKeywordsRule : BaseSqlCodeAnalysisRule
    {
        private readonly HashSet<string> sqlWords;

        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0067";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Use capitalized keywords for enhanced readability.";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = "Capitalize the keyword '{0}' for enhanced readability.";

        /// <summary>
        /// Initializes a new instance of the <see cref="UseCapitalizedKeywordsRule"/> class.
        /// </summary>
        public UseCapitalizedKeywordsRule()
            : base(ModelSchema.Procedure, ModelSchema.Table, ModelSchema.View, ModelSchema.ScalarFunction, ModelSchema.TableValuedFunction)
        {
            sqlWords = new HashSet<string>(TSqlKeywords.Concat(TSqlDataTypes), StringComparer.OrdinalIgnoreCase);
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

                var text = token.Text;

                if (string.IsNullOrWhiteSpace(text))
                {
                    continue;
                }

                if (token.TokenType == TSqlTokenType.QuotedIdentifier && text.Length > 2)
                {
                    text = text.Substring(1, text.Length - 2);
                }

                if (text.All(char.IsWhiteSpace))
                {
                    continue;
                }

                if (!text.All(char.IsLetter))
                {
                    continue;
                }

                if (IsUpperCase(text))
                {
                    continue;
                }

                if (sqlWords.Contains(text))
                {
                    problems.Add(new SqlRuleProblem(MessageFormatter.FormatMessage(string.Format(CultureInfo.InvariantCulture, Message, text), RuleId), sqlObj, fragment));
                }
            }

            return problems;
        }

        private static bool IsUpperCase(string input)
        {
            return input.All(char.IsUpper);
        }
    }
}