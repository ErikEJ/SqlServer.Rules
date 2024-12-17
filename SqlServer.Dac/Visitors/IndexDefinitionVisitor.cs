using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;

namespace SqlServer.Dac.Visitors
{
    public class IndexDefinitionVisitor : BaseVisitor, IVisitor<IndexDefinition>
    {
        public IList<IndexDefinition> Statements { get; } = new List<IndexDefinition>();
        public int Count { get { return Statements.Count; } }
        public override void ExplicitVisit(IndexDefinition node)
        {
            Statements.Add(node);
        }
    }
}