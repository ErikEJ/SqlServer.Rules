using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Performance
{
    /// <summary>
    /// Consider using <c>RECOMPILE</c> query hint instead of <c>WITH RECOMPILE</c> option
    /// </summary>
    /// <FriendlyName>Procedure level recompile option</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd>
    ///    good:
    ///     ```sql
    ///     CREATE PROCEDURE dbo.my_proc
    ///     BEGIN
    ///         SELECT col_A, col_b
    ///         FROM some_complicated_set
    ///         WHERE some_complicated_filter = 1
    ///         OPTION(RECOMPILE)
    ///     ```
    ///
    ///    bad:
    ///     ```sql
    ///     CREATE PROCEDURE dbo.my_proc
    ///     WITH RECOMPILE
    ///     ```
    /// </ExampleMd>
    /// <remarks>
    /// The rule checks that stored procedures do not use <c>WITH RECOMPILE</c> procedure option. The
    /// <c>OPTION(RECOMPILE)</c> is the preferred method when a recompile is needed. The <c>WITH RECOMPILE</c>
    /// procedure option instructs the Database Engine does not cache a plan for this procedure and
    /// the procedure is compiled at run time.
    /// </remarks>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Performance,
        RuleScope = SqlRuleScope.Element)]
    public sealed class ConsiderRecompileQueryHintRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRP0022";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Consider using RECOMPILE query hint instead of the WITH RECOMPILE option.";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsiderRecompileQueryHintRule"/> class.
        /// </summary>
        public ConsiderRecompileQueryHintRule()
            : base(ModelSchema.Procedure)
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

            var proc = ruleExecutionContext.ScriptFragment?.GetFragment(typeof(CreateProcedureStatement)) as CreateProcedureStatement;

            if (proc == null)
            {
                return problems;
            }

            if (proc.Options.Any(o =>
                o.OptionKind == ProcedureOptionKind.Recompile)
                && Ignorables.ShouldNotIgnoreRule(proc.ScriptTokenStream, RuleId, proc.StartLine))
            {
                problems.Add(new SqlRuleProblem(MessageFormatter.FormatMessage(Message, RuleId), sqlObj));
            }

            return problems;
        }
    }
}
