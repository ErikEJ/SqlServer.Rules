using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public class TransactionVisitor : BaseVisitor, IVisitor<TransactionStatement>
    {
        public IList<TransactionStatement> Statements { get; } = new List<TransactionStatement>();
        public int Count
        {
            get { return Statements.Count; }
        }

        public override void Visit(TransactionStatement node)
        {
            Statements.Add(node);
        }
    }
}