using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public class GrantVisitor : BaseVisitor, IVisitor<GrantStatement>
    {
        public IList<GrantStatement> Statements { get; } = new List<GrantStatement>();
        public int Count { get { return Statements.Count; } }
        public override void ExplicitVisit(GrantStatement node)
        {
            Statements.Add(node);
        }
    }
}