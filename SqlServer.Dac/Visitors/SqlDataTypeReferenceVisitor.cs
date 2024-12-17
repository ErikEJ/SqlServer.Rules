using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;

namespace SqlServer.Dac.Visitors
{
    public class SqlDataTypeReferenceVisitor : BaseVisitor, IVisitor<SqlDataTypeReference>
    {
        public IList<SqlDataTypeReference> Statements { get; } = new List<SqlDataTypeReference>();
        public int Count { get { return Statements.Count; } }
        public override void ExplicitVisit(SqlDataTypeReference node)
        {
            Statements.Add(node);
        }
    }
}
