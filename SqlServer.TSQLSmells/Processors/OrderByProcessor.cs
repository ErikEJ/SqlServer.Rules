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

        private void ProcessOrderExpression(ExpressionWithSortOrder Expression)
        {
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
            var SubExpressionType = FragmentTypeParser.GetFragmentType(Expression.Expression);
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
            switch (SubExpressionType)
            {
                case "IntegerLiteral":
                    smells.SendFeedBack(7, Expression);
                    break;
                case "CastCall":
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
                    var CastCall = (CastCall)Expression.Expression;
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
                    if (FragmentTypeParser.GetFragmentType(CastCall.Parameter) == "ColumnReferenceExpression")
                    {
                        smells.SendFeedBack(6, Expression);
                    }

                    break;
            }
        }

        public void Process(OrderByClause OrderClause)
        {
            if (OrderClause == null)
            {
                return;
            }

#pragma warning disable SA1312 // Variable names should begin with lower-case letter
            foreach (var Expression in OrderClause.OrderByElements)
            {
                ProcessOrderExpression(Expression);
            }
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
        }
    }
}