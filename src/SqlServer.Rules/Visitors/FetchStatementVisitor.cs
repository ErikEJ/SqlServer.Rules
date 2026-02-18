using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public class FetchStatementVisitor : BaseVisitor, IVisitor<FetchCursorStatement>
    {
        public IList<FetchCursorStatement> Statements { get; } = new List<FetchCursorStatement>();

        public int Count => Statements.Count;

        public override void Visit(FetchCursorStatement node)
        {
            Statements.Add(node);
        }
    }
}
