using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;

namespace SqlServer.Dac.Visitors
{
    public class CreateIndexStatementVisitor : BaseVisitor, IVisitor<CreateIndexStatement>
    {
        public ObjectTypeFilter TypeFilter { get; set; } = ObjectTypeFilter.All;
        public IList<CreateIndexStatement> Statements { get; } = new List<CreateIndexStatement>();
        public int Count { get { return Statements.Count; } }
        public override void Visit(CreateIndexStatement node)
        {
            switch (TypeFilter)
            {
                case ObjectTypeFilter.PermanentOnly:
                    if (!node.OnName.GetName().Contains("#")) Statements.Add(node);
                    break;

                case ObjectTypeFilter.TempOnly:
                    if (node.OnName.GetName().Contains("#")) Statements.Add(node);
                    break;

                default:
                    Statements.Add(node);
                    break;
            }
        }
    }
}