using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public class CreateUserVisitor : BaseVisitor, IVisitor<CreateUserStatement>
    {
        public IList<CreateUserStatement> Statements { get; } = new List<CreateUserStatement>();

        public int Count
        {
            get { return Statements.Count; }
        }

        public override void ExplicitVisit(CreateUserStatement node)
        {
            Statements.Add(node);
        }
    }
}
