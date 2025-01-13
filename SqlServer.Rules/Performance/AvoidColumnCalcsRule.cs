using System.Collections.Generic;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Performance
{
    /// <summary>
    /// Avoid the use of calculations on columns in the where clause.
    /// </summary>
    /// <FriendlyName>Avoid Column Calculations</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Performance,
        RuleScope = SqlRuleScope.Element)]
    public sealed class AvoidColumnCalcsRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRP0015";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Avoid the use of calculations on columns in the where clause. (Sargable)";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;


        /// <summary>
        /// Initializes a new instance of the <see cref="AvoidColumnCalcsRule"/> class.
        /// </summary>
        public AvoidColumnCalcsRule() : base(ProgrammingAndViewSchemas)
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
                var binaryExpressionVisitor = new BinaryExpressionVisitor();
                whereClause.Accept(binaryExpressionVisitor);

                foreach (var comparison in binaryExpressionVisitor.NotIgnoredStatements(RuleId))
                {
                    if (CheckBinaryExpression(comparison).GetValueOrDefault(false))
                    {
                        problems.Add(new SqlRuleProblem(Message, sqlObj, comparison));
                    }
                }
            }

            return problems;
        }

        private static bool? CheckBinaryExpression(BinaryExpression bin)
        {
            bool? ret = null;

            if (
                (bin.FirstExpression.GetType() == typeof(ColumnReferenceExpression)
                    || bin.SecondExpression.GetType() == typeof(ColumnReferenceExpression))
                && (bin.FirstExpression.GetType() == typeof(IntegerLiteral)
                    || bin.SecondExpression.GetType() == typeof(IntegerLiteral)))
            {
                return true;
            }

            if (!ret.HasValue && bin.FirstExpression is BinaryExpression expression)
            {
                ret = CheckBinaryExpression(expression);
            }

            if (!ret.HasValue && bin.SecondExpression is BinaryExpression binaryExpression)
            {
                ret = CheckBinaryExpression(binaryExpression);
            }

            return ret;
        }
    }
}
