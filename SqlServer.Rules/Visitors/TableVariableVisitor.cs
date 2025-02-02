using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public class TableVariableVisitor : BaseVisitor, IVisitor<VariableTableReference>
    {
        public IList<VariableTableReference> Statements { get; } = new List<VariableTableReference>();

        public int Count
        {
            get { return Statements.Count; }
        }

        public override void ExplicitVisit(VariableTableReference node)
        {
            Statements.Add(node);
        }
    }
}