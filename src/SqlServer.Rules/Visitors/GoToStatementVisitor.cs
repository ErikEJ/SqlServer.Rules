using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public class GoToStatementVisitor : BaseVisitor, IVisitor<GoToStatement>
    {
        public IList<GoToStatement> Statements { get; } = new List<GoToStatement>();

        public int Count => Statements.Count;

        public override void Visit(GoToStatement node)
        {
            Statements.Add(node);
        }
    }
}
