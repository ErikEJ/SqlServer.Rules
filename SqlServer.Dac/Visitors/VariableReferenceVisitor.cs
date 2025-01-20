using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public class VariableReferenceVisitor : BaseVisitor, IVisitor<VariableReference>
    {
        public IList<VariableReference> Statements { get; } = new List<VariableReference>();
        public int Count
        {
            get { return Statements.Count; }
        }

        public override void Visit(VariableReference node)
        {
            Statements.Add(node);
        }
    }
}