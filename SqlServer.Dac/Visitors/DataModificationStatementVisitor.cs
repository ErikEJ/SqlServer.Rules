using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public class DataModificationStatementVisitor : BaseVisitor, IVisitor<DataModificationStatement>
    {
        public IList<DataModificationStatement> Statements { get; } = new List<DataModificationStatement>();

        public int Count
        {
            get { return Statements.Count; }
        }

        public override void Visit(DataModificationStatement node)
        {
            Statements.Add(node);
        }
    }
}