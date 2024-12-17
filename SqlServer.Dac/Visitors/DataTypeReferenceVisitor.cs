using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;

namespace SqlServer.Dac.Visitors
{
    public class DataTypeReferenceVisitor : BaseVisitor, IVisitor<DataTypeReference>
    {
        public IList<DataTypeReference> Statements { get; } = new List<DataTypeReference>();
        public int Count { get { return Statements.Count; } }
        public override void ExplicitVisit(DataTypeReference node)
        {
            Statements.Add(node);
        }
    }
}
