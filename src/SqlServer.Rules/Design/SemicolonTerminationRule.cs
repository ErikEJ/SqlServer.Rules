using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Design
{
    /// <summary>
    /// Terminate statements with semicolon to follow Microsoft guidelines.
    /// </summary>
    /// <FriendlyName>Terminate statements with semicolon.</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class SemicolonTerminationRule : BaseSqlCodeAnalysisRule
    {
        private readonly Type[] typesToSkip =
        {
            typeof(BeginEndBlockStatement),
            typeof(GoToStatement),
            typeof(IndexDefinition),
            typeof(LabelStatement),
            typeof(WhileStatement),
            typeof(IfStatement),
            typeof(CreateViewStatement),
        };

        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0068";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Query statements should finish with a semicolon - ';'.";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="SemicolonTerminationRule"/> class.
        /// </summary>
        public SemicolonTerminationRule()
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

            var fragment = ruleExecutionContext.ScriptFragment?.GetFragment(ProgrammingAndViewSchemaTypes);

            if (fragment == null)
            {
                return problems;
            }

            var waitforVisitor = new WaitForVisitor();
            var statementVisitor = new StatementVisitor();
            var functionSelectVisitor = new CreateFunctionSelectVisitor();

            fragment.Accept(waitforVisitor, statementVisitor, functionSelectVisitor);

            foreach (var statement in statementVisitor.Statements)
            {
                if (typesToSkip.Contains(statement.GetType())
                    || EndsWithSemicolon(statement)
                    || functionSelectVisitor.Statements.Contains(statement)
                    || waitforVisitor.Statements.Contains(statement))
                {
                    continue;
                }

                problems.Add(new SqlRuleProblem(MessageFormatter.FormatMessage(Message, RuleId), sqlObj, statement));
            }

            return problems;
        }

        private static bool EndsWithSemicolon(TSqlStatement node)
        {
            return node.ScriptTokenStream[node.LastTokenIndex].TokenType == TSqlTokenType.Semicolon
                || node.ScriptTokenStream[node.LastTokenIndex + 1].TokenType == TSqlTokenType.Semicolon;
        }
    }
}
