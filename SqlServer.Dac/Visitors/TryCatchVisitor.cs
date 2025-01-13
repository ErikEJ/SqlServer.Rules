using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public class TryCatchVisitor : BaseVisitor, IVisitor<TryCatchStatement>
    {
        public IList<TryCatchStatement> Statements { get; } = new List<TryCatchStatement>();
        public int Count { get { return Statements.Count; } }
        public override void Visit(TryCatchStatement node)
        {
            Statements.Add(node);
        }
    }
}