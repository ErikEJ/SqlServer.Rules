using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public class SelectElementVisitor : BaseVisitor, IVisitor<SelectElement>
    {
        public IList<SelectElement> Statements { get; } = new List<SelectElement>();
        public int Count { get { return Statements.Count; } }
        public override void ExplicitVisit(SelectElement node)
        {
            Statements.Add(node);
        }
    }
}
