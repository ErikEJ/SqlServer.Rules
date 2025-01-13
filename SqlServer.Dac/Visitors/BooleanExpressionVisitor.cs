using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public class BooleanExpressionVisitor : BaseVisitor, IVisitor<BooleanExpression>
    {
        public IList<BooleanExpression> Statements { get; } = new List<BooleanExpression>();
        public int Count { get { return Statements.Count; } }
        public override void Visit(BooleanExpression node)
        {
            Statements.Add(node);
        }
    }
}