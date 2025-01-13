using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Performance
{
    /// <summary>
    /// Avoid the use of functions with UPDATE / INSERT  / DELETE statements. (Halloween Protection)
    /// </summary>
    /// <FriendlyName>Function in data modification</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// When a user defined function that does not use <c>SCHEMABINDING</c> is used in an action
    /// query the data modifications have to be spooled to tempdb in a two step operation.
    /// </remarks>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Performance,
        RuleScope = SqlRuleScope.Element)]
    public sealed class AvoidFunctionsInActionQueries : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRP0010";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Avoid the use of user defined functions with UPDATE/INSERT/DELETE statements. (Halloween Protection)";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="AvoidFunctionsInActionQueries"/> class.
        /// </summary>
        public AvoidFunctionsInActionQueries() : base(ProgrammingSchemas)
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
            if (sqlObj == null || sqlObj.IsWhiteListed()) { return problems; }
            var model = ruleExecutionContext.SchemaModel;
            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(ProgrammingSchemaTypes);

            var visitor = new DataModificationStatementVisitor();
            fragment.Accept(visitor);

            var modelFunctions = model.GetObjects(DacQueryScopes.UserDefined, ModelSchema.ScalarFunction, ModelSchema.TableValuedFunction);

            foreach (var stmt in visitor.Statements)
            {
                var functionCallVisitor = new FunctionCallVisitor();
                stmt.Accept(functionCallVisitor);

                foreach (var functionCall in functionCallVisitor.NotIgnoredStatements(RuleId))
                {
                    var createFunctionVisitor = new CreateFunctionVisitor();
                    TSqlFragment fnFragment;

                    var fnName = functionCall.GetName();
                    var modelFunction = modelFunctions.FirstOrDefault(mf => Comparer.Equals(mf.Name.GetName(), fnName));
                    if (modelFunction == null) { continue; }

                    // we need to parse the SQL into a fragment, so we can use the visitors on it
                    fnFragment = modelFunction.GetFragment(out var parseErrors);
                    fnFragment.Accept(createFunctionVisitor);

                    if (!createFunctionVisitor.Statements.Any(crfn => crfn.Options != null && crfn.Options.Any(o => o.OptionKind == FunctionOptionKind.SchemaBinding)))
                    {
                        problems.Add(new SqlRuleProblem(Message, sqlObj, functionCall));
                    }
                }
            }


            return problems;
        }
    }
}