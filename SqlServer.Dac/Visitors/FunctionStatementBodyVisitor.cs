using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public class FunctionStatementBodyVisitor : BaseVisitor, IVisitor<FunctionStatementBody>
    {
        public IList<FunctionStatementBody> Statements { get; } = new List<FunctionStatementBody>();

        public int Count
        {
            get { return Statements.Count; }
        }

        public override void ExplicitVisit(FunctionStatementBody node)
        {
            Statements.Add(node);
        }
    }
}