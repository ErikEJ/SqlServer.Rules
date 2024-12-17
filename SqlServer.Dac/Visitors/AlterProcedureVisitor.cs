using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;

namespace SqlServer.Dac.Visitors
{
    public class AlterProcedureVisitor : BaseVisitor, IVisitor<AlterProcedureStatement>
    {
        public ObjectTypeFilter TypeFilter { get; set; } = ObjectTypeFilter.All;
        public IList<AlterProcedureStatement> Statements { get; } = new List<AlterProcedureStatement>();
        public int Count { get { return Statements.Count; } }
        public override void Visit(AlterProcedureStatement node)
        {
            switch (TypeFilter)
            {
                case ObjectTypeFilter.PermanentOnly:
                    if (!node.ProcedureReference.Name.GetName().Contains("#")) Statements.Add(node);
                    break;

                case ObjectTypeFilter.TempOnly:
                    if (node.ProcedureReference.Name.GetName().Contains("#")) Statements.Add(node);
                    break;

                default:
                    Statements.Add(node);
                    break;
            }
        }
    }
}