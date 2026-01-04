using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public class BeginTransactionVisitor : BaseVisitor, IVisitor<BeginTransactionStatement>
    {
        public IList<BeginTransactionStatement> Statements { get; } = new List<BeginTransactionStatement>();

        public int Count
        {
            get { return Statements.Count; }
        }

        public override void Visit(BeginTransactionStatement node)
        {
            Statements.Add(node);
        }
    }
}