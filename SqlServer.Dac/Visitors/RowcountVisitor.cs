using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public class RowCountVisitor : BaseVisitor, IVisitor<SetRowCountStatement>
    {
        public IList<SetRowCountStatement> Statements { get; } = new List<SetRowCountStatement>();
        public int Count { get { return Statements.Count; } }
        public override void ExplicitVisit(SetRowCountStatement node)
        {
            Statements.Add(node);
        }
    }
}