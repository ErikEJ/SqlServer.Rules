using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;

namespace SqlServer.Dac.Visitors
{
    public class UpdateVisitor : BaseVisitor, IVisitor<UpdateStatement>
    {
        public IList<UpdateStatement> Statements { get; } = new List<UpdateStatement>();
        public int Count { get { return Statements.Count; } }
        public override void ExplicitVisit(UpdateStatement node)
        {
            Statements.Add(node);
        }
    }
}