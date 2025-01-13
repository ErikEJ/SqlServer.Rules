using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public class ColumnDefinitionVisitor : BaseVisitor, IVisitor<ColumnDefinition>
    {
        public IList<ColumnDefinition> Statements { get; } = new List<ColumnDefinition>();
        public int Count { get { return Statements.Count; } }
        public override void ExplicitVisit(ColumnDefinition node)
        {
            Statements.Add(node);
        }
    }
}