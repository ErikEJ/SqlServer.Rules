using System;
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
    /// Try to avoid the OR operator in query where clauses if possible. This affects sargability.
    /// </summary>
    /// <FriendlyName>Avoid use of OR in where clause</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class AvoidORInQueriesRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0032";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Try to avoid the OR operator in query where clauses if possible.  (Sargable)";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="AvoidORInQueriesRule"/> class.
        /// </summary>
        public AvoidORInQueriesRule()
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

            // only visit the operators in the where clause
            var visitor = new WhereClauseVisitor();
            fragment.Accept(visitor);

            foreach (var clause in visitor.Statements)
            {
                var beVisitor = new BooleanParenthesesExpressionVisitor();
                clause.Accept(beVisitor);

                var offenders = (from BooleanParenthesisExpression be in beVisitor.NotIgnoredStatements(RuleId)
                                 where TestClause(be)
                                 select be).ToList();

                problems.AddRange(offenders.Select(o => new SqlRuleProblem(Message, sqlObj, o)));
            }

            return problems;
        }

        private static bool TestClause(BooleanParenthesisExpression be)
        {
            if (be.Expression is BooleanBinaryExpression booleanExpression)
            {
                var expr1 = booleanExpression.FirstExpression;
                var expr2 = booleanExpression.SecondExpression;

                return booleanExpression.BinaryExpressionType == BooleanBinaryExpressionType.Or
                    && (expr1 is BooleanIsNullExpression || expr1 is BooleanComparisonExpression)
                    && (expr2 is BooleanIsNullExpression || expr2 is BooleanComparisonExpression)
                    && Comparer.Equals(GetVariableName(expr1), GetVariableName(expr2));
            }

            return false;
        }

        private static string GetVariableName(BooleanExpression bex)
        {
            var ret = Guid.NewGuid().ToString();
            if (bex is BooleanIsNullExpression nullExpr && nullExpr.Expression is VariableReference varExpr)
            {
                ret = varExpr.Name;
            }

            if (bex is BooleanComparisonExpression compareExpr)
            {
                if (compareExpr.FirstExpression is VariableReference expr1)
                {
                    ret = expr1.Name;
                }
                else if (compareExpr.SecondExpression is VariableReference expr2)
                {
                    ret = expr2.Name;
                }
            }

            return ret;
        }
    }
}