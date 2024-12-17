using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;

namespace SqlServer.Dac.Visitors
{
    public class CreateFunctionVisitor : BaseVisitor, IVisitor<CreateFunctionStatement>
    {
        public IList<CreateFunctionStatement> Statements { get; } = new List<CreateFunctionStatement>();
        public int Count { get { return Statements.Count; } }
        public override void ExplicitVisit(CreateFunctionStatement node)
        {
            Statements.Add(node);
        }
    }
}
