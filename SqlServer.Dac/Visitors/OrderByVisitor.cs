using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public class OrderByVisitor : BaseVisitor, IVisitor<OrderByClause>
    {
        public IList<OrderByClause> Statements { get; } = new List<OrderByClause>();
        public int Count { get { return Statements.Count; } }
        public override void ExplicitVisit(OrderByClause node)
        {
            Statements.Add(node);
        }
    }
}