using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;

namespace SqlServer.Dac.Visitors
{
    public class NamedTableReferenceVisitor : BaseVisitor, IVisitor<NamedTableReference>
    {
        public ObjectTypeFilter TypeFilter { get; set; } = ObjectTypeFilter.All;
        public IList<NamedTableReference> Statements { get; } = new List<NamedTableReference>();
        public int Count { get { return Statements.Count; } }
        public override void ExplicitVisit(NamedTableReference node)
        {
            switch (TypeFilter)
            {
                case ObjectTypeFilter.PermanentOnly:
                    if (!node.GetName().Contains("#")) Statements.Add(node);
                    break;

                case ObjectTypeFilter.TempOnly:
                    if (node.GetName().Contains("#")) Statements.Add(node);
                    break;

                default:
                    Statements.Add(node);
                    break;
            }
        }
    }
}