using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;

namespace SqlServer.Dac.Visitors
{
    public class FromClauseVisitor : BaseVisitor, IVisitor<FromClause>
    {
        public IList<FromClause> Statements { get; } = new List<FromClause>();
        public int Count { get { return Statements.Count; } }
        public override void ExplicitVisit(FromClause node)
        {
            Statements.Add(node);
        }
    }
}