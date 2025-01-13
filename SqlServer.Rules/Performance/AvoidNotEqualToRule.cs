using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Performance
{
    /// <summary>
    ///  Avoid using not equal operator (&lt;>,!=) in the WHERE clause. (Sargeable) 
    /// </summary>
    /// <FriendlyName>Use of inequality</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// The rule checks for usage of the not equal operator in the WHERE clause as it result table
    /// and index scans. Consider replacing the not equal operator with equals (=) or inequality
    /// operators (>,>=,&lt;,&lt;=) if possible.
    /// </remarks>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Performance,
        RuleScope = SqlRuleScope.Element)]
    public sealed class AvoidNotEqualToRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRP0006";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Try to avoid using not equal operator (<>,!=) in the WHERE clause if possible. (Sargable)";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="AvoidNotEqualToRule"/> class.
        /// </summary>
        public AvoidNotEqualToRule() : base(ProgrammingAndViewSchemas)
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

            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(ProgrammingAndViewSchemaTypes);

            var whereClauseVisitor = new WhereClauseVisitor();
            fragment.Accept(whereClauseVisitor);

            foreach (var whereClause in whereClauseVisitor.Statements)
            {
                var booleanComparisonVisitor = new BooleanComparisonVisitor();
                whereClause.Accept(booleanComparisonVisitor);

                var offenders = booleanComparisonVisitor.NotIgnoredStatements(RuleId)
                    .Where(c =>
                        c.ComparisonType == BooleanComparisonType.NotEqualToBrackets
                        || c.ComparisonType == BooleanComparisonType.NotEqualToExclamation);

                var sqlObjName = ruleExecutionContext.GetObjectName(sqlObj);
                problems.AddRange(offenders.Select(t => new SqlRuleProblem(Message, sqlObj, t)));
            }

            return problems;
        }
    }
}