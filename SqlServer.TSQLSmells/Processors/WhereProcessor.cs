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

        private void ProcessWhereBooleanExpression(BooleanExpression booleanExpression)
        {
            var expressionType = FragmentTypeParser.GetFragmentType(booleanExpression);
            switch (expressionType)
            {
                case "BooleanComparisonExpression":
                    var boolComp = (BooleanComparisonExpression)booleanExpression;
                    ProcessWhereScalarExpression(boolComp.FirstExpression);
                    ProcessWhereScalarExpression(boolComp.SecondExpression);
                    if ((boolComp.ComparisonType == BooleanComparisonType.Equals) &&
                        (FragmentTypeParser.GetFragmentType(boolComp.FirstExpression) == "NullLiteral" ||
                         FragmentTypeParser.GetFragmentType(boolComp.SecondExpression) == "NullLiteral")
                       )
                    {
                        smells.SendFeedBack(46, boolComp);
                    }

                    break;
                case "BooleanBinaryExpression":
                    var boolExpression = (BooleanBinaryExpression)booleanExpression;
                    ProcessWhereBooleanExpression(boolExpression.FirstExpression);
                    ProcessWhereBooleanExpression(boolExpression.SecondExpression);
                    break;
                default:
                    break;
            }
        }

        private void ProcessWhereScalarExpression(ScalarExpression whereExpression)
        {
            var expressionType = FragmentTypeParser.GetFragmentType(whereExpression);
            string parameterType;
            switch (expressionType)
            {
                case "ConvertCall":
                    var convertCall = (ConvertCall)whereExpression;
                    parameterType = FragmentTypeParser.GetFragmentType(convertCall.Parameter);
                    if (parameterType == "ColumnReferenceExpression")
                    {
                        smells.SendFeedBack(6, convertCall);
                    }

                    break;
                case "CastCall":
                    var castCall = (CastCall)whereExpression;
                    parameterType = FragmentTypeParser.GetFragmentType(castCall.Parameter);
                    if (parameterType == "ColumnReferenceExpression")
                    {
                        smells.SendFeedBack(6, castCall);
                    }

                    break;
                case "ScalarSubquery":
                    var subQuery = (ScalarSubquery)whereExpression;
                    smells.ProcessQueryExpression(subQuery.QueryExpression, "RG");
                    break;
            }
        }

        public void Process(WhereClause whereClause)
        {
            if (whereClause?.SearchCondition != null)
            {
                ProcessWhereBooleanExpression(whereClause.SearchCondition);
            }
        }
    }
}