using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public class ScalarSubqueryVisitor : BaseVisitor, IVisitor<ScalarSubquery>
    {
        public IList<ScalarSubquery> Statements { get; } = new List<ScalarSubquery>();
        public int Count
        {
            get { return Statements.Count; }
        }

        public override void Visit(ScalarSubquery node)
        {
            Statements.Add(node);
        }
    }
}