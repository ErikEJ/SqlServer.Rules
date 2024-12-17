using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;

namespace SqlServer.Dac.Visitors
{
    public class SetVariableStatementVisitor : BaseVisitor, IVisitor<SetVariableStatement>
    {
        public IList<SetVariableStatement> Statements { get; } = new List<SetVariableStatement>();
        public int Count { get { return Statements.Count; } }
        public override void Visit(SetVariableStatement node)
        {
            Statements.Add(node);
        }
    }
}