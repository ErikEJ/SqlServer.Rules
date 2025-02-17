using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Performance
{
    /// <summary>
    /// Avoid using patterns that start with '%' with the LIKE keyword  (Sargeable)
    /// </summary>
    /// <FriendlyName>Unanchored string pattern</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// This rule checks for usage of wildcard characters at the beginning of a word while searching
    /// using the LIKE keyword. Usage of wildcard characters at the beginning of a LIKE pattern
    /// results in an index scan, which defeats the purpose of an index.
    /// </remarks>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Performance,
        RuleScope = SqlRuleScope.Element)]
    public sealed class AvoidEndsWithOrContainsRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRP0002";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Try to avoid using patterns that start with '%' when using the LIKE keyword if possible.  (Sargable)";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="AvoidEndsWithOrContainsRule"/> class.
        /// </summary>
        public AvoidEndsWithOrContainsRule()
            : base(ProgrammingAndViewSchemas)
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

            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(ProgrammingAndViewSchemaTypes);

            var whereClauseVisitor = new WhereClauseVisitor();
            fragment.Accept(whereClauseVisitor);

            foreach (var whereClause in whereClauseVisitor.Statements)
            {
                var likeVisitor = new LikePredicateVisitor();
                whereClause.Accept(likeVisitor);

                foreach (var like in likeVisitor.NotIgnoredStatements(RuleId))
                {
                    var stringLiteralVisitor = new StringLiteralVisitor();
                    like.Accept(stringLiteralVisitor);

                    var literal = stringLiteralVisitor.NotIgnoredStatements(RuleId)
                        .FirstOrDefault(l => l.Value.StartsWith('%') && l.Value.Length > 1);

                    if (literal != null)
                    {
                        problems.Add(new SqlRuleProblem(MessageFormatter.FormatMessage(Message, RuleId), sqlObj, like));
                        break;
                    }
                }
            }

            return problems;
        }
    }
}
