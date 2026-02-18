using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public class ExistsPredicateVisitor : BaseVisitor, IVisitor<ExistsPredicate>
    {
        public IList<ExistsPredicate> Statements { get; } = new List<ExistsPredicate>();

        public int Count => Statements.Count;

        public override void Visit(ExistsPredicate node)
        {
            Statements.Add(node);
        }
    }
}
