using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public class ParameterVisitor : BaseVisitor, IVisitor<ProcedureParameter>
    {
        public IList<ProcedureParameter> Statements { get; } = new List<ProcedureParameter>();
        public int Count
        {
            get { return Statements.Count; }
        }

        public override void ExplicitVisit(ProcedureParameter node)
        {
            Statements.Add(node);
        }
    }
}