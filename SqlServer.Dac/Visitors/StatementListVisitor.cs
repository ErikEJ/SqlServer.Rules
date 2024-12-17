using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;

namespace SqlServer.Dac.Visitors
{
    public class StatementListVisitor : BaseVisitor, IVisitor<StatementList>
    {
        public IList<StatementList> Statements { get; } = new List<StatementList>();
        public int Count { get { return Statements.Count; } }
        public override void ExplicitVisit(StatementList node)
        {
            Statements.Add(node);
        }
    }
}