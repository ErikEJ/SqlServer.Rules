using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Design
{
    /// <summary>
    /// Removed unreferenced parameters
    /// </summary>
    /// <FriendlyName>Unused input parameter</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// This rule checks for not used stored procedure or function input parameters.
    /// Unused parameters not necessarily negatively affect the performance, but they just add bloat
    /// to your stored procedures and functions.
    /// </remarks>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    /// <seealso cref="SqlServer.Rules.Design.TypesMissingParametersRule" />
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class ConsiderRemovingUnusedParameterRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0016";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Input parameter never used. Consider removing the parameter or using it.";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = "Input parameter '{0}' is never used. Consider removing the parameter or using it.";

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsiderRemovingUnusedParameterRule"/> class.
        /// </summary>
        public ConsiderRemovingUnusedParameterRule()
            : base(ProgrammingSchemas)
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

            var fragment = sqlObj.GetFragment();
            if (fragment.ScriptTokenStream == null)
            {
                return problems;
            }

            var visitor = new VariablesVisitor();
            fragment.Accept(visitor);

#pragma warning disable CA1304 // Specify CultureInfo
#pragma warning disable CA1311 // Specify a culture or use an invariant version
            var parms = from pp in visitor.ProcedureParameters
                        join t in fragment.ScriptTokenStream
                            on new { Name = pp.VariableName.Value?.ToLower(), Type = TSqlTokenType.Variable }
                            equals new { Name = t.Text?.ToLower(), Type = t.TokenType }
                        where Ignorables.ShouldNotIgnoreRule(fragment.ScriptTokenStream, RuleId, pp.StartLine)
                        select pp;

            var unusedParms = parms.GroupBy(p => p.VariableName.Value?.ToLower())
                .Where(g => g.Count() == 1).Select(g => g.First());
#pragma warning restore CA1304 // Specify CultureInfo
#pragma warning restore CA1311 // Specify a culture or use an invariant version
            problems.AddRange(unusedParms.Select(rp => new SqlRuleProblem(MessageFormatter.FormatMessage(string.Format(CultureInfo.InvariantCulture, Message, rp.VariableName.Value), RuleId), sqlObj, rp)));

            return problems;
        }
    }
}