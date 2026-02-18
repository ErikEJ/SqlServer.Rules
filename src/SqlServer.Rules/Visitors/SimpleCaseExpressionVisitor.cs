using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public class SimpleCaseExpressionVisitor : BaseVisitor, IVisitor<SimpleCaseExpression>
    {
        public IList<SimpleCaseExpression> Statements { get; } = new List<SimpleCaseExpression>();

        public int Count => Statements.Count;

        public override void Visit(SimpleCaseExpression node)
        {
            Statements.Add(node);
        }
    }
}
