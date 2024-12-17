using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;

namespace SqlServer.Dac.Visitors
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Microsoft.SqlServer.TransactSql.ScriptDom.TSqlFragmentVisitor" />
    public class UseDatabaseVisitor : BaseVisitor, IVisitor<UseStatement>
    {
        public IList<UseStatement> Statements { get; } = new List<UseStatement>();
        public int Count { get { return Statements.Count; } }
        public override void ExplicitVisit(UseStatement node)
        {
            Statements.Add(node);
        }
    }
}
