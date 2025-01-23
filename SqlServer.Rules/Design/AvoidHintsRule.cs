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
    /// Avoid the use of HINTS to enforce a particular behavior in your code.
    /// </summary>
    /// <FriendlyName>Avoid Use of HINTS</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class AvoidHintsRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0030";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Avoid using Hints to force a particular behavior.";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="AvoidHintsRule"/> class.
        /// </summary>
        public AvoidHintsRule()
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

            var tableHintVisitor = new TableHintVisitor();
            var queryHintVisitor = new StatementListVisitor();
            var joinHintVisitor = new JoinVisitor();

            fragment.Accept(tableHintVisitor, queryHintVisitor, joinHintVisitor);

            var tableOffenders =
               from n in tableHintVisitor.NotIgnoredStatements(RuleId)
               where n.HintKind != TableHintKind.NoLock
               select n;

            var queryOffenders =
                from o in queryHintVisitor.NotIgnoredStatements(RuleId)
                from o1 in o.Statements
                let s = o1 as StatementWithCtesAndXmlNamespaces
                where s?.OptimizerHints != null && s?.OptimizerHints.Count > 0
                select s;

            var joinOffenders =
                from j in joinHintVisitor.NotIgnoredStatements(RuleId)
                where j is QualifiedJoin @join
                    && @join.JoinHint != JoinHint.None
                select j as QualifiedJoin;

            problems.AddRange(tableOffenders.Select(o => new SqlRuleProblem(MessageFormatter.FormatMessage(Message, RuleId), sqlObj, o)));
            problems.AddRange(queryOffenders.Select(o => new SqlRuleProblem(MessageFormatter.FormatMessage(Message, RuleId), sqlObj, o)));
            problems.AddRange(joinOffenders.Select(o => new SqlRuleProblem(MessageFormatter.FormatMessage(Message, RuleId), sqlObj, o)));

            return problems;
        }
    }
}