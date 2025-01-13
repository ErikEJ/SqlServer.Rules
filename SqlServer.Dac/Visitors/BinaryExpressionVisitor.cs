using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public class BinaryExpressionVisitor : BaseVisitor, IVisitor<BinaryExpression>
    {
        public IList<BinaryExpression> Statements { get; } = new List<BinaryExpression>();
        public int Count { get { return Statements.Count; } }
        public override void ExplicitVisit(BinaryExpression node)
        {
            Statements.Add(node);
        }
    }
}