using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;

namespace SqlServer.Dac.Visitors
{
    public class SelectStarExpressionVisitor : BaseVisitor, IVisitor<SelectStarExpression>
    {
        public IList<SelectStarExpression> Statements { get; } = new List<SelectStarExpression>();
        public int Count { get { return Statements.Count; } }
        public override void ExplicitVisit(SelectStarExpression node)
        {
            Statements.Add(node);
        }
    }
}
