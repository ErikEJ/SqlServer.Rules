using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public class OpenCursorVisitor : BaseVisitor, IVisitor<OpenCursorStatement>
    {
        public IList<OpenCursorStatement> Statements { get; } = new List<OpenCursorStatement>();
        public int Count { get { return Statements.Count; } }
        public override void ExplicitVisit(OpenCursorStatement node)
        {
            Statements.Add(node);
        }
    }
}