using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Microsoft.SqlServer.TransactSql.ScriptDom.TSqlFragmentVisitor" />
    public class CreateRoleVisitor : BaseVisitor, IVisitor<CreateRoleStatement>
    {
        public IList<CreateRoleStatement> Statements { get; } = new List<CreateRoleStatement>();
        public int Count { get { return Statements.Count; } }
        public override void ExplicitVisit(CreateRoleStatement node)
        {
            Statements.Add(node);
        }
    }
}
