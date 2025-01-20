using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TSQLSmellSCA
{
    public class WhereProcessor
    {
        private readonly Smells smells;

        public WhereProcessor(Smells smells)
        {
            this.smells = smells;
        }

        private void ProcessWhereBooleanExpression(BooleanExpression BooleanExpression)
        {
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
            var ExpressionType = FragmentTypeParser.GetFragmentType(BooleanExpression);
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
            switch (ExpressionType)
            {
                case "BooleanComparisonExpression":
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
                    var BoolComp = (BooleanComparisonExpression)BooleanExpression;
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
                    ProcessWhereScalarExpression(BoolComp.FirstExpression);
                    ProcessWhereScalarExpression(BoolComp.SecondExpression);
                    if ((BoolComp.ComparisonType == BooleanComparisonType.Equals) &&
                        (FragmentTypeParser.GetFragmentType(BoolComp.FirstExpression) == "NullLiteral" ||
                         FragmentTypeParser.GetFragmentType(BoolComp.SecondExpression) == "NullLiteral")
                       )
                    {
                        smells.SendFeedBack(46, BoolComp);
                    }

                    break;
                case "BooleanBinaryExpression":
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
                    var BoolExpression = (BooleanBinaryExpression)BooleanExpression;
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
                    ProcessWhereBooleanExpression(BoolExpression.FirstExpression);
                    ProcessWhereBooleanExpression(BoolExpression.SecondExpression);
                    break;
                default:
                    break;
            }
        }

        private void ProcessWhereScalarExpression(ScalarExpression WhereExpression)
        {
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
            var ExpressionType = FragmentTypeParser.GetFragmentType(WhereExpression);
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
            string ParameterType;
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
            switch (ExpressionType)
            {
                case "ConvertCall":
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
                    var ConvertCall = (ConvertCall)WhereExpression;
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
                    ParameterType = FragmentTypeParser.GetFragmentType(ConvertCall.Parameter);
                    if (ParameterType == "ColumnReferenceExpression")
                    {
                        smells.SendFeedBack(6, ConvertCall);
                    }

                    break;
                case "CastCall":
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
                    var CastCall = (CastCall)WhereExpression;
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
                    ParameterType = FragmentTypeParser.GetFragmentType(CastCall.Parameter);
                    if (ParameterType == "ColumnReferenceExpression")
                    {
                        smells.SendFeedBack(6, CastCall);
                    }

                    break;
                case "ScalarSubquery":
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
                    var SubQuery = (ScalarSubquery)WhereExpression;
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
                    smells.ProcessQueryExpression(SubQuery.QueryExpression, "RG");
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