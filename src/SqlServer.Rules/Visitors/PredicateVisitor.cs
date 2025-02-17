using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    // used to check for the existence of set options turned on that match Microsoft.SqlServer.TransactSql.ScriptDom.SetOptions
    public class PredicateVisitor : BaseVisitor, IVisitor<PredicateSetStatement>
    {
        public IList<PredicateSetStatement> Statements { get; } = new List<PredicateSetStatement>();

        public int Count
        {
            get { return Statements.Count; }
        }

        public override void ExplicitVisit(PredicateSetStatement node)
        {
            Statements.Add(node);
        }
    }
}
