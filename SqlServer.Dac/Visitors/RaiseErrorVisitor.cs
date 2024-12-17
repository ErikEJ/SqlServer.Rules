using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;

namespace SqlServer.Dac.Visitors
{
    public class RaiseErrorVisitor : BaseVisitor, IVisitor<RaiseErrorStatement>
    {
        public IList<RaiseErrorStatement> Statements { get; } = new List<RaiseErrorStatement>();
        public int Count { get { return Statements.Count; } }
        public override void ExplicitVisit(RaiseErrorStatement node)
        {
            Statements.Add(node);
        }
    }
}