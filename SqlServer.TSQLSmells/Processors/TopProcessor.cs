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

        public void ProcessTopFilter(TopRowFilter TopFilter)
        {
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
            IntegerLiteral TopLiteral = null;
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
            if (FragmentTypeParser.GetFragmentType(TopFilter.Expression) != "ParenthesisExpression")
            {
                smells.SendFeedBack(34, TopFilter);
                if (FragmentTypeParser.GetFragmentType(TopFilter.Expression) == "IntegerLiteral")
                {
                    TopLiteral = (IntegerLiteral)TopFilter.Expression;
                }
            }
            else
            {
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
                var ParenthesisExpression = (ParenthesisExpression)TopFilter.Expression;
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
                if (FragmentTypeParser.GetFragmentType(ParenthesisExpression.Expression) == "IntegerLiteral")
                {
                    TopLiteral = (IntegerLiteral)ParenthesisExpression.Expression;
                }
            }

            if (TopFilter.Percent && TopLiteral != null && TopLiteral.Value == "100")
            {
                smells.SendFeedBack(35, TopLiteral);
            }
        }
    }
}