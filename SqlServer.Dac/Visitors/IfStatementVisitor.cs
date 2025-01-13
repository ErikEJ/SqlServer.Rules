using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public class IfStatementVisitor : BaseVisitor, IVisitor<IfStatement>
    {
        public IList<IfStatement> Statements { get; } = new List<IfStatement>();
        public int Count { get { return Statements.Count; } }
        public override void ExplicitVisit(IfStatement node)
        {
            Statements.Add(node);
        }
    }
}