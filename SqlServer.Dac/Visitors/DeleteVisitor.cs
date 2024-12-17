using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;

namespace SqlServer.Dac.Visitors
{
    public class DeleteVisitor : BaseVisitor, IVisitor<DeleteStatement>
    {
        public IList<DeleteStatement> Statements { get; } = new List<DeleteStatement>();
        public int Count { get { return Statements.Count; } }
        public override void ExplicitVisit(DeleteStatement node)
        {
            Statements.Add(node);
        }
    }
}