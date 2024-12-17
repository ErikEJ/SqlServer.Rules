using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;
using System.Collections.Generic;
using System.Linq;

namespace SqlServer.Rules.Design
{
    /// <summary>
    ///
    /// </summary>
    /// <FriendlyName></FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public class ConsiderCachingGetDateToVariable : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0064";
        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Cache multiple calls to GETDATE or SYSDATETIME into a variable.";
        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        private readonly List<string> FunctionNames = new List<string> { "getdate", "sysdatetime" };

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsiderCachingGetDateToVariable"/> class.
        /// </summary>
        public ConsiderCachingGetDateToVariable() : base(ProgrammingSchemas)
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
            var candidates = new List<StatementWithCtesAndXmlNamespaces>();
            var sqlObj = ruleExecutionContext.ModelElement;

            if (sqlObj == null)
                return problems;

            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(ProgrammingSchemaTypes);

            List<StatementWithCtesAndXmlNamespaces> statements = new List<StatementWithCtesAndXmlNamespaces>();

            var selectVisitor = new SelectStatementVisitor();
            fragment.Accept(selectVisitor);
            statements.AddRange(selectVisitor.NotIgnoredStatements(RuleId));

            var actionStatementVisitor = new ActionStatementVisitor();
            fragment.Accept(actionStatementVisitor);
            statements.AddRange(actionStatementVisitor.NotIgnoredStatements(RuleId));

            if (statements.Count > 1)
            {
                statements.ForEach(statement =>
                {
                    if (DoesStatementHaveDateFunction(statement)) { candidates.Add(statement); }
                });
            }

            problems.AddRange(candidates.Select(s => new SqlRuleProblem(Message, sqlObj, s)));

            return problems;
        }

        private bool DoesStatementHaveDateFunction(StatementWithCtesAndXmlNamespaces statement)
        {
            bool hasDateFunction;

            var allFunctions = new FunctionCallVisitor();

            statement.Accept(allFunctions);

            if (allFunctions.Statements.Any(p => FunctionNames.Contains(p.FunctionName.Value.ToLower())))
            {
                hasDateFunction = true;
            }
            else
            {
                hasDateFunction = CheckFunctionCallsForDateFunction(allFunctions.Statements);
            }

            return hasDateFunction;
        }

        private bool CheckFunctionCallsForDateFunction(IList<FunctionCall> functionCalls)
        {
            bool hasDateFunctions = false;

            foreach (var functionCall in functionCalls)
            {
                if (FunctionNames.Contains(functionCall.FunctionName.Value.ToLower()))
                {
                    hasDateFunctions = true;
                }
                else
                {
                    foreach (var param in functionCall.Parameters)
                    {
                        var functionVisitor = new FunctionCallVisitor();
                        param.Accept(functionVisitor);
                        hasDateFunctions = hasDateFunctions || CheckFunctionCallsForDateFunction(functionVisitor.Statements);
                        if (hasDateFunctions) { break; }
                    }
                }
                if (hasDateFunctions) { break; }
            }

            return hasDateFunctions;
        }
    }
}