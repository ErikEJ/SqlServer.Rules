using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;

namespace SqlServer.Dac.Visitors
{
    public class WhereClauseVisitor : BaseVisitor, IVisitor<WhereClause>
    {
        public IList<WhereClause> Statements { get; } = new List<WhereClause>();
        public int Count { get { return Statements.Count; } }
        public override void ExplicitVisit(WhereClause node)
        {
            Statements.Add(node);
        }
    }
}