using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public class SelectStatementVisitor : BaseVisitor, IVisitor<SelectStatement>
    {
        public IList<SelectStatement> Statements { get; } = new List<SelectStatement>();
        public int Count { get { return Statements.Count; } }
        public override void ExplicitVisit(SelectStatement node)
        {
            Statements.Add(node);
        }
    }
}