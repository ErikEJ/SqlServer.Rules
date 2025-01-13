using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public class InPredicateVisitor : BaseVisitor, IVisitor<InPredicate>
    {
        public IList<InPredicate> Statements { get; } = new List<InPredicate>();
        public int Count { get { return Statements.Count; } }
        public override void ExplicitVisit(InPredicate node)
        {
            Statements.Add(node);
        }
    }
}
