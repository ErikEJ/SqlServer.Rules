using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public class SchemaObjectNameVisitor : BaseVisitor, IVisitor<SchemaObjectName>
    {
        public IList<SchemaObjectName> Statements { get; } = new List<SchemaObjectName>();
        public int Count { get { return Statements.Count; } }
        public override void ExplicitVisit(SchemaObjectName node)
        {
            Statements.Add(node);
        }
    }
}