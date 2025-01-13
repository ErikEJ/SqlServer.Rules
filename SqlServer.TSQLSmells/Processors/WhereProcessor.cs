using System;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TSQLSmellSCA
{
    public class WhereProcessor
    {
        private readonly Smells _smells;

        public WhereProcessor(Smells smells)
        {
            _smells = smells;
        }

        private void ProcessWhereBooleanExpression(BooleanExpression BooleanExpression)
        {
            var ExpressionType = FragmentTypeParser.GetFragmentType(BooleanExpression);
            switch (ExpressionType)
            {
                case "BooleanComparisonExpression":
                    var BoolComp = (BooleanComparisonExpression)BooleanExpression;
                    ProcessWhereScalarExpression(BoolComp.FirstExpression);
                    ProcessWhereScalarExpression(BoolComp.SecondExpression);
                    if ((BoolComp.ComparisonType == BooleanComparisonType.Equals) &&
                        (FragmentTypeParser.GetFragmentType(BoolComp.FirstExpression) == "NullLiteral" ||
                         FragmentTypeParser.GetFragmentType(BoolComp.SecondExpression) == "NullLiteral")
                       )
                    {
                        _smells.SendFeedBack(46, BoolComp);
                    }

                    break;
                case "BooleanBinaryExpression":
                    var BoolExpression = (BooleanBinaryExpression)BooleanExpression;
                    ProcessWhereBooleanExpression(BoolExpression.FirstExpression);
                    ProcessWhereBooleanExpression(BoolExpression.SecondExpression);
                    break;
                default:
                    break;
            }
        }

        private void ProcessWhereScalarExpression(ScalarExpression WhereExpression)
        {
            var ExpressionType = FragmentTypeParser.GetFragmentType(WhereExpression);
            String ParameterType;
            switch (ExpressionType)
            {
                case "ConvertCall":
                    var ConvertCall = (ConvertCall)WhereExpression;
                    ParameterType = FragmentTypeParser.GetFragmentType(ConvertCall.Parameter);
                    if (ParameterType == "ColumnReferenceExpression")
                    {
                        _smells.SendFeedBack(6, ConvertCall);
                    }

                    break;
                case "CastCall":
                    var CastCall = (CastCall)WhereExpression;
                    ParameterType = FragmentTypeParser.GetFragmentType(CastCall.Parameter);
                    if (ParameterType == "ColumnReferenceExpression")
                    {
                        _smells.SendFeedBack(6, CastCall);
                    }

                    break;
                case "ScalarSubquery":
                    var SubQuery = (ScalarSubquery)WhereExpression;
                    _smells.ProcessQueryExpression(SubQuery.QueryExpression, "RG");
                    break;
            }
        }

        public void Process(WhereClause WhereClause)
        {
            if (WhereClause?.SearchCondition != null)
            {
                ProcessWhereBooleanExpression(WhereClause.SearchCondition);
            }
        }
    }
}