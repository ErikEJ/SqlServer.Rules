using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public class QueryHintVisitor : BaseVisitor, IVisitor<QuerySpecification>
    {
        public IList<QuerySpecification> Statements { get; } = new List<QuerySpecification>();

        public int Count
        {
            get { return Statements.Count; }
        }

        public override void ExplicitVisit(QuerySpecification node)
        {
            Statements.Add(node);
        }
    }
}