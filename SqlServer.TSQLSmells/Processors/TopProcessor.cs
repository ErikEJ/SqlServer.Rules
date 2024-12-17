using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TSQLSmellSCA
{
    public class TopProcessor
    {
        private readonly Smells _smells;

        public TopProcessor(Smells smells)
        {
            _smells = smells;
        }

        public void ProcessTopFilter(TopRowFilter TopFilter)
        {
            IntegerLiteral TopLiteral = null;
            if (FragmentTypeParser.GetFragmentType(TopFilter.Expression) != "ParenthesisExpression")
            {
                _smells.SendFeedBack(34, TopFilter);
                if (FragmentTypeParser.GetFragmentType(TopFilter.Expression) == "IntegerLiteral")
                {
                    TopLiteral = (IntegerLiteral)TopFilter.Expression;
                }
            }
            else
            {
                var ParenthesisExpression = (ParenthesisExpression)TopFilter.Expression;
                if (FragmentTypeParser.GetFragmentType(ParenthesisExpression.Expression) == "IntegerLiteral")
                {
                    TopLiteral = (IntegerLiteral)ParenthesisExpression.Expression;
                }
            }

            if (TopFilter.Percent && TopLiteral != null && TopLiteral.Value == "100")
            {
                _smells.SendFeedBack(35, TopLiteral);
            }
        }
    }
}