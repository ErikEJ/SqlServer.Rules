using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public class ConvertCallVisitor : BaseVisitor, IVisitor<ConvertCall>
    {
        public IList<ConvertCall> Statements { get; } = new List<ConvertCall>();

        public int Count
        {
            get { return Statements.Count; }
        }

        public override void Visit(ConvertCall node)
        {
            Statements.Add(node);
        }
    }
}