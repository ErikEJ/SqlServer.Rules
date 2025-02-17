using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public class TopRowFilterVisitor : BaseVisitor, IVisitor<TopRowFilter>
    {
        public IList<TopRowFilter> Statements { get; } = new List<TopRowFilter>();

        public int Count
        {
            get { return Statements.Count; }
        }

        public override void ExplicitVisit(TopRowFilter node)
        {
            Statements.Add(node);
        }
    }
}