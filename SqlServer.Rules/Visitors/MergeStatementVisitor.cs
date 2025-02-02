using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public class MergeStatementVisitor : BaseVisitor, IVisitor<MergeStatement>
    {
        public IList<MergeStatement> Statements { get; } = new List<MergeStatement>();

        public int Count
        {
            get { return Statements.Count; }
        }

        public override void ExplicitVisit(MergeStatement node)
        {
            Statements.Add(node);
        }
    }
}