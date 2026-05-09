using System.Collections.Generic;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Performance
{
    /// <summary>
    /// Avoid explicit conversion of columnar data in the WHERE clause.
    /// </summary>
    /// <FriendlyName>Avoid explicit conversion of columnar data</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Performance,
        RuleScope = SqlRuleScope.Element)]
    public sealed class AvoidExplicitColumnConversionRule : BaseSqlCodeAnalysisRule
    {
        public const string RuleId = Constants.RuleNameSpace + "SRP0027";
        public const string RuleDisplayName = "Avoid explicit conversion of columnar data in the WHERE clause. (Sargable)";
        public const string Message = RuleDisplayName;

        public AvoidExplicitColumnConversionRule()
            : base(ProgrammingAndViewSchemas)
        {
        }

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

            var whereClauseVisitor = new WhereClauseVisitor();
            fragment.Accept(whereClauseVisitor);

            foreach (var whereClause in whereClauseVisitor.Statements)
            {
                var booleanComparisonVisitor = new BooleanComparisonVisitor();
                whereClause.Accept(booleanComparisonVisitor);

                foreach (var comparison in booleanComparisonVisitor.NotIgnoredStatements(RuleId))
                {
                    if (IsExplicitColumnConversion(comparison.FirstExpression)
                        || IsExplicitColumnConversion(comparison.SecondExpression))
                    {
                        problems.Add(new SqlRuleProblem(MessageFormatter.FormatMessage(Message, RuleId), sqlObj, comparison));
                    }
                }
            }

            return problems;
        }

        private static bool IsExplicitColumnConversion(ScalarExpression expression)
        {
            if (expression is ConvertCall convertCall)
            {
                return convertCall.Parameter is ColumnReferenceExpression;
            }

            if (expression is CastCall castCall)
            {
                return castCall.Parameter is ColumnReferenceExpression;
            }

            return false;
        }
    }
}
