using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public class UseDatabaseVisitor : BaseVisitor, IVisitor<UseStatement>
    {
        public IList<UseStatement> Statements { get; } = new List<UseStatement>();

        public int Count
        {
            get { return Statements.Count; }
        }

        public override void ExplicitVisit(UseStatement node)
        {
            Statements.Add(node);
        }
    }
}
