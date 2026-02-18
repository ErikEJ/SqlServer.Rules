using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public class DeclareCursorStatementVisitor : BaseVisitor, IVisitor<DeclareCursorStatement>
    {
        public IList<DeclareCursorStatement> Statements { get; } = new List<DeclareCursorStatement>();

        public int Count => Statements.Count;

        public override void Visit(DeclareCursorStatement node)
        {
            Statements.Add(node);
        }
    }
}
