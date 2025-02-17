using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public class BooleanParenthesesExpressionVisitor : BaseVisitor, IVisitor<BooleanParenthesisExpression>
    {
        public IList<BooleanParenthesisExpression> Statements { get; } = new List<BooleanParenthesisExpression>();

        public int Count
        {
            get { return Statements.Count; }
        }

        public override void ExplicitVisit(BooleanParenthesisExpression node)
        {
            Statements.Add(node);
        }
    }
}