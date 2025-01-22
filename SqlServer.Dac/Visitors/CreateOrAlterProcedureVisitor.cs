using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public class CreateOrAlterProcedureVisitor : BaseVisitor, IVisitor<CreateOrAlterProcedureStatement>
    {
        public ObjectTypeFilter TypeFilter { get; set; } = ObjectTypeFilter.All;

        public IList<CreateOrAlterProcedureStatement> Statements { get; } = new List<CreateOrAlterProcedureStatement>();

        public int Count
        {
            get { return Statements.Count; }
        }

        public override void Visit(CreateOrAlterProcedureStatement node)
        {
            switch (TypeFilter)
            {
                case ObjectTypeFilter.PermanentOnly:
                    if (!node.ProcedureReference.Name.GetName().Contains('#', System.StringComparison.OrdinalIgnoreCase))
                    {
                        Statements.Add(node);
                    }

                    break;

                case ObjectTypeFilter.TempOnly:
                    if (node.ProcedureReference.Name.GetName().Contains('#', System.StringComparison.OrdinalIgnoreCase))
                    {
                        Statements.Add(node);
                    }

                    break;

                default:
                    Statements.Add(node);
                    break;
            }
        }
    }
}