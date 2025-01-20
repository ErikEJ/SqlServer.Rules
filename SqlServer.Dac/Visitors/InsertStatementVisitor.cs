using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public class InsertStatementVisitor : BaseVisitor, IVisitor<InsertStatement>
    {
        public IList<InsertStatement> Statements { get; } = new List<InsertStatement>();
        public int Count
        {
            get { return Statements.Count; }
        }

        public override void Visit(InsertStatement node)
        {
            Statements.Add(node);
        }
    }
}