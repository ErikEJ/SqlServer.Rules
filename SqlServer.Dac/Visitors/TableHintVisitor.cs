using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public class TableHintVisitor : BaseVisitor, IVisitor<TableHint>
    {
        public IList<TableHint> Statements { get; } = new List<TableHint>();
        public int Count
        {
            get { return Statements.Count; }
        }

        public override void ExplicitVisit(TableHint node)
        {
            // TODO: Does not visit FORCESEEK and possible others
            Statements.Add(node);
        }
    }
}