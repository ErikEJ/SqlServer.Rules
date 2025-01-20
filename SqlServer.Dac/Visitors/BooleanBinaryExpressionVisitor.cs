using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public class BooleanBinaryExpressionVisitor : BaseVisitor, IVisitor<BooleanBinaryExpression>
    {
        public IList<BooleanBinaryExpression> Statements { get; } = new List<BooleanBinaryExpression>();
        public int Count
        {
            get { return Statements.Count; }
        }

        public override void ExplicitVisit(BooleanBinaryExpression node)
        {
            Statements.Add(node);
        }
    }
}