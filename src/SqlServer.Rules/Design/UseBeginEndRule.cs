using System.Collections.Generic;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Design
{
    /// <summary>
    /// Use BEGIN and END symbols inside conditional statements.
    /// </summary>
    /// <FriendlyName>BEGIN and END symbols inside conditional statements.</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd>
    /// Examples of **incorrect** code for this rule:
    /// ```tsql
    ///  IF(@parm = 1)
    ///    SELECT @output = 'foo'
    /// ```
    /// Examples of **correct** code for this rule:
    /// ```tsql
    ///  IF (@parm = 1)
    ///  BEGIN
    ///    SELECT @output = 'foo'
    ///  END
    /// ```
    /// </ExampleMd>
    /// <remarks>
    /// Use BEGIN and END to bind conditional statments as a single block of code.
    /// </remarks>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class UseBeginEndRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0066";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Use BEGIN and END symbols inside conditional statements.";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = "Use BEGIN and END symbols inside conditional statements.";

        /// <summary>
        /// Initializes a new instance of the <see cref="UseBeginEndRule"/> class.
        /// </summary>
        public UseBeginEndRule()
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

            var ifVisitor = new IfStatementVisitor();

            fragment.Accept(ifVisitor);

            if (ifVisitor.Statements.Count == 0)
            {
                return problems;
            }

            foreach (var ifStatement in ifVisitor.NotIgnoredStatements(RuleId))
            {
                if (ifStatement.ThenStatement != null && ifStatement.ThenStatement is not BeginEndBlockStatement)
                {
                    problems.Add(new SqlRuleProblem(MessageFormatter.FormatMessage(Message, RuleId), sqlObj, ifStatement.ThenStatement));
                }

                if (ifStatement.ElseStatement != null && ifStatement.ElseStatement is not BeginEndBlockStatement && ifStatement.ElseStatement is not IfStatement)
                {
                    problems.Add(new SqlRuleProblem(MessageFormatter.FormatMessage(Message, RuleId), sqlObj, ifStatement.ElseStatement));
                }
            }

            return problems;
        }
    }
}