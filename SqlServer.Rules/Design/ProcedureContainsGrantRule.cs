using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Design
{
    /// <summary>
    /// The procedure grants itself permissions. Possible missing GO command
    /// </summary>
    /// <FriendlyName>Permission change in stored procedure</FriendlyName>
    /// <IsIgnorable>false</IsIgnorable>
    /// <ExampleMd>
    ///    ```sql
    ///     CREATE PROCEDURE dbo.my_proc 
    ///     AS
    ///         SELECT some_columns, some_calc 
    ///         FROM some_set
    ///         WHERE 1=0
    ///         /* GO; */ /* &lt; you might want one of these */
    ///         GRANT exec to some_one
    ///    ```
    /// </ExampleMd>
    /// <remarks>
    /// The rule checks for stored procedures, changing its own permissions. It is possible that a
    /// GO end of batch signaling command is missing and the statements in the script following the
    /// procedure are included in the procedure body.
    /// </remarks>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class ProcedureContainsGrantRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0060";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "The procedure grants itself permissions. Possible missing GO command.";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcedureContainsGrantRule"/> class.
        /// </summary>
        public ProcedureContainsGrantRule() : base(ModelSchema.Procedure)
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

            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(typeof(CreateProcedureStatement));
            var objName = sqlObj.Name.GetName();

            var grantVisitor = new GrantVisitor();
            fragment.Accept(grantVisitor);

            if (grantVisitor.Statements
                .Any(g => g.SecurityTargetObject.ObjectName.MultiPartIdentifier.CompareTo(sqlObj.Name) >= 5))
            {
                problems.Add(new SqlRuleProblem(Message, sqlObj));
            }

            return problems;
        }
    }
}