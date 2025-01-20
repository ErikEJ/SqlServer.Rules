using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public class LikePredicateVisitor : BaseVisitor, IVisitor<LikePredicate>
    {
        public IList<LikePredicate> Statements { get; } = new List<LikePredicate>();
        public int Count
        {
            get { return Statements.Count; }
        }

        public override void ExplicitVisit(LikePredicate node)
        {
            Statements.Add(node);
        }
    }
}
