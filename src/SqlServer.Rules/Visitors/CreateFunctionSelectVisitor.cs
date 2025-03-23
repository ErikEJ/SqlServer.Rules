using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public class CreateFunctionSelectVisitor : BaseVisitor, IVisitor<SelectStatement>
    {
        public IList<SelectStatement> Statements { get; } = new List<SelectStatement>();

        public int Count
        {
            get { return Statements.Count; }
        }

        public override void ExplicitVisit(CreateFunctionStatement node)
        {
            if (node.ReturnType is SelectFunctionReturnType returnType)
            {
                Statements.Add(returnType.SelectStatement);
            }
        }
    }
}
