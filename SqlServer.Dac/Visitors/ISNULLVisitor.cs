using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;

namespace SqlServer.Dac.Visitors
{
    public class ISNULLVisitor : BaseVisitor, IVisitor<NullIfExpression>
    {
        public IList<NullIfExpression> Statements { get; } = new List<NullIfExpression>();
        public int Count { get { return Statements.Count; } }
        public override void ExplicitVisit(NullIfExpression node)
        {
            Statements.Add(node);
        }
    }
}