using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TSQLSmellSCA
{
    public class OrderByProcessor
    {
        private readonly Smells smells;

        public OrderByProcessor(Smells smells)
        {
            this.smells = smells;
        }

        private void ProcessOrderExpression(ExpressionWithSortOrder expression)
        {
            var subExpressionType = FragmentTypeParser.GetFragmentType(expression.Expression);
            switch (subExpressionType)
            {
                case "IntegerLiteral":
                    smells.SendFeedBack(7, expression);
                    break;
                case "CastCall":
                    var castCall = (CastCall)expression.Expression;
                    if (FragmentTypeParser.GetFragmentType(castCall.Parameter) == "ColumnReferenceExpression")
                    {
                        smells.SendFeedBack(6, expression);
                    }

                    break;
            }
        }

        public void Process(OrderByClause orderClause)
        {
            if (orderClause == null)
            {
                return;
            }

            foreach (var expression in orderClause.OrderByElements)
            {
                ProcessOrderExpression(expression);
            }
        }
    }
}