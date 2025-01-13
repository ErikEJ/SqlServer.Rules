using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public class DeclareVariableElementVisitor : BaseVisitor, IVisitor<DeclareVariableElement>
    {
        public IList<DeclareVariableElement> Statements { get; } = new List<DeclareVariableElement>();
        public int Count { get { return Statements.Count; } }
        public override void ExplicitVisit(DeclareVariableElement node)
        {
            Statements.Add(node);
        }
    }
}