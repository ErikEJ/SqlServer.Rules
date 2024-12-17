using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;

namespace SqlServer.Dac.Visitors
{
    public class ExecutableStringListVisitor : BaseVisitor, IVisitor<ExecutableStringList>
    {
        public IList<ExecutableStringList> Statements { get; } = new List<ExecutableStringList>();
        public int Count { get { return Statements.Count; } }
        public override void ExplicitVisit(ExecutableStringList node)
        {
            Statements.Add(node);
        }
    }
}