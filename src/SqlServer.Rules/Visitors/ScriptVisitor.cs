using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public class ScriptVisitor : BaseVisitor, IVisitor<TSqlScript>
    {
        public IList<TSqlScript> Statements { get; } = new List<TSqlScript>();

        public int Count
        {
            get { return Statements.Count; }
        }

        public override void ExplicitVisit(TSqlScript node)
        {
            Statements.Add(node);
        }
    }
}
