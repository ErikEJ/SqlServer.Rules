using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TSQLSmellSCA
{
    public class TopProcessor
    {
        private readonly Smells smells;

        public TopProcessor(Smells smells)
        {
            this.smells = smells;
        }

        public void ProcessTopFilter(TopRowFilter topFilter)
        {
            IntegerLiteral topLiteral = null;
            if (FragmentTypeParser.GetFragmentType(topFilter.Expression) != "ParenthesisExpression")
            {
                smells.SendFeedBack(34, topFilter);
                if (FragmentTypeParser.GetFragmentType(topFilter.Expression) == "IntegerLiteral")
                {
                    topLiteral = (IntegerLiteral)topFilter.Expression;
                }
            }
            else
            {
                var parenthesisExpression = (ParenthesisExpression)topFilter.Expression;
                if (FragmentTypeParser.GetFragmentType(parenthesisExpression.Expression) == "IntegerLiteral")
                {
                    topLiteral = (IntegerLiteral)parenthesisExpression.Expression;
                }
            }

            if (topFilter.Percent && topLiteral != null && topLiteral.Value == "100")
            {
                smells.SendFeedBack(35, topLiteral);
            }
        }
    }
}