using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public class StatementVisitor : BaseVisitor, IVisitor<TSqlStatement>
    {
        public IList<TSqlStatement> Statements { get; } = new List<TSqlStatement>();

        public int Count
        {
            get { return Statements.Count; }
        }

        public override void Visit(TSqlStatement node)
        {
            Statements.Add(node);
        }
    }
}