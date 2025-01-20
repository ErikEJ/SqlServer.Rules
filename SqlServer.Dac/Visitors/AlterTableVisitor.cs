using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public class AlterTableVisitor : BaseVisitor, IVisitor<AlterTableStatement>
    {
        public ObjectTypeFilter TypeFilter { get; set; } = ObjectTypeFilter.All;
        public IList<AlterTableStatement> Statements { get; } = new List<AlterTableStatement>();
        public int Count
        {
            get { return Statements.Count; }
        }

        public override void Visit(AlterTableStatement node)
        {
            switch (TypeFilter)
            {
                case ObjectTypeFilter.PermanentOnly:
                    if (!node.SchemaObjectName.GetName().Contains('#', System.StringComparison.OrdinalIgnoreCase))
                    {
                        Statements.Add(node);
                    }

                    break;

                case ObjectTypeFilter.TempOnly:
                    if (node.SchemaObjectName.GetName().Contains('#', System.StringComparison.OrdinalIgnoreCase))
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