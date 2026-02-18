using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public class BooleanNotExpressionVisitor : BaseVisitor, IVisitor<BooleanNotExpression>
    {
        public IList<BooleanNotExpression> Statements { get; } = new List<BooleanNotExpression>();

        public int Count => Statements.Count;

        public override void Visit(BooleanNotExpression node)
        {
            Statements.Add(node);
        }
    }
}
