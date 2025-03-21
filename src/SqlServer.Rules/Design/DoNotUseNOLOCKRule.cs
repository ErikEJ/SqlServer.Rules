using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Design
{
    /// <summary>Do not use the NOLOCK clause</summary>
    /// <FriendlyName>Use of NOLOCK</FriendlyName>
    /// <IsIgnorable>false</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// - **Dirty read** - this is the one most people are aware of; you can read data that has not been committed, and could be rolled back some time after you've read it - meaning you've read data that never technically existed.
    /// - Missing rows - because of the way an allocation scan works, other transactions could move data you haven't read yet to an earlier location in the chain that you've already read, or add a new page behind the scan, meaning you won't see it at all.
    /// - Reading rows twice - bimilarly, data that you've already read could be moved to a later location in the chain, meaning you will read it twice.
    /// - Reading multiple versions of the same row - when using READ UNCOMMITTED, you can get a version of a row that never existed; for example, where you see some columns that have been changed by concurrent users, but you don't see their changes reflected in all columns. This can even happen within a single column (see a great example from Paul White).
    /// - Index corruption - surely you are not using NOLOCK in INSERT/UPDATE/DELETE statements, but if you are, you should be aware that this syntax is deprecated and that it can cause corruption, even in SQL Server 2014 RTM - see this tip for more information. Note that you should check for the hint in any views that you are trying to update, too.
    /// </remarks>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class DoNotUseNOLOCKRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0034";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Do not use the NOLOCK clause.";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="DoNotUseNOLOCKRule"/> class.
        /// </summary>
        public DoNotUseNOLOCKRule()
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

            var visitor = new TableHintVisitor();

            fragment.Accept(visitor);

            var offenders =
                from n in visitor.Statements
                where n.HintKind == TableHintKind.NoLock
                select n;

            problems.AddRange(offenders.Select(o => new SqlRuleProblem(MessageFormatter.FormatMessage(Message, RuleId), sqlObj, o)));

            return problems;
        }
    }
}