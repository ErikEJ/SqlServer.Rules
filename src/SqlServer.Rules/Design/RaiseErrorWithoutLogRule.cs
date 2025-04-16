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
    /// The RAISERROR statement with severity above 18 and requires WITH LOG clause.
    /// </summary>
    /// <FriendlyName>Error handling requires SA permissions</FriendlyName>
    /// <IsIgnorable>false</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// The rule checks RAISERROR statements for having severity above 18 and not having a
    /// <c>WITH LOG</c> clause. Error severity levels greater than 18 can only be specified by
    /// members of the sysadmin role, using the WITH LOG option. Severity levels from 0 through 18
    /// can be specified by any user. Severity levels from 19 through 25 can only be specified by
    /// members of the sysadmin fixed server role or users with <c>ALTER TRACE</c> permissions.
    /// For severity levels from 19 through 25, the <c>WITH LOG</c> option is required.
    /// </remarks>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class RaiseErrorWithoutLogRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0044";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "The RAISERROR statement with severity above 18 requires the WITH LOG clause.";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="RaiseErrorWithoutLogRule"/> class.
        /// </summary>
        public RaiseErrorWithoutLogRule()
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

            var fragment = ruleExecutionContext.ScriptFragment?.GetFragment(ProgrammingSchemaTypes);

            if (fragment == null)
            {
                return problems;
            }

            var visitor = new RaiseErrorVisitor();

            fragment.Accept(visitor);

            var offenders =
                from r in visitor.Statements
                where
                    r.SecondParameter is IntegerLiteral literal &&
                    literal.Value != null &&
                    int.Parse(literal.Value, CultureInfo.InvariantCulture) > 18 &&
                    (r.RaiseErrorOptions & RaiseErrorOptions.Log) != RaiseErrorOptions.Log
                select r;

            problems.AddRange(offenders.Select(o => new SqlRuleProblem(MessageFormatter.FormatMessage(Message, RuleId), sqlObj, o)));

            return problems;
        }
    }
}