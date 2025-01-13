using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public class WaitForVisitor : BaseVisitor, IVisitor<WaitForStatement>
    {
        public IList<WaitForStatement> Statements { get; } = new List<WaitForStatement>();
        public int Count { get { return Statements.Count; } }
        public override void ExplicitVisit(WaitForStatement node)
        {
            Statements.Add(node);
        }
    }
}