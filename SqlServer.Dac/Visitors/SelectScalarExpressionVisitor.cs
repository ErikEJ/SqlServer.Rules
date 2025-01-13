using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public class SelectScalarExpressionVisitor : BaseVisitor, IVisitor<SelectScalarExpression>
    {
        public IList<SelectScalarExpression> Statements { get; } = new List<SelectScalarExpression>();
        public int Count { get { return Statements.Count; } }
        public override void Visit(SelectScalarExpression node)
        {
            Statements.Add(node);
        }
    }
}