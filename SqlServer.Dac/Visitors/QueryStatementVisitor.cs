using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;

namespace SqlServer.Dac.Visitors
{
    public class QueryStatementVisitor : BaseVisitor, IVisitor<StatementWithCtesAndXmlNamespaces>
    {
        public IList<StatementWithCtesAndXmlNamespaces> Statements { get; } = new List<StatementWithCtesAndXmlNamespaces>();
        public int Count { get { return Statements.Count; } }
        public override void Visit(StatementWithCtesAndXmlNamespaces node)
        {
            Statements.Add(node);
        }
    }
}