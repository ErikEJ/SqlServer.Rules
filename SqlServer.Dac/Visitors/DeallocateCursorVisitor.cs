using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public class DeallocateCursorVisitor : BaseVisitor, IVisitor<DeallocateCursorStatement>
    {
        public IList<DeallocateCursorStatement> Statements { get; } = new List<DeallocateCursorStatement>();
        public int Count
        {
            get { return Statements.Count; }
        }

        public override void ExplicitVisit(DeallocateCursorStatement node)
        {
            Statements.Add(node);
        }
    }
}