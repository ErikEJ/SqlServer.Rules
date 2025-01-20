using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public class StringLiteralVisitor : BaseVisitor, IVisitor<StringLiteral>
    {
        public IList<StringLiteral> Statements { get; } = new List<StringLiteral>();
        public int Count
        {
            get { return Statements.Count; }
        }

        public override void ExplicitVisit(StringLiteral node)
        {
            Statements.Add(node);
        }
    }
}
