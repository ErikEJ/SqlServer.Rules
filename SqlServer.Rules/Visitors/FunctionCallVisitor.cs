using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public class FunctionCallVisitor : BaseVisitor, IVisitor<FunctionCall>
    {
        private readonly IList<string> functionNames;

        public FunctionCallVisitor()
        {
            functionNames = new List<string>();
        }

        public FunctionCallVisitor(params string[] functionNames)
        {
            this.functionNames = functionNames.ToList();
        }

        public IList<FunctionCall> Statements { get; } = new List<FunctionCall>();

        public int Count
        {
            get { return Statements.Count; }
        }

        public override void ExplicitVisit(FunctionCall node)
        {
            if (!functionNames.Any())
            {
                Statements.Add(node);
            }
            else if (functionNames.Any(f => Comparer.Equals(f, node.FunctionName.Value)))
            {
                Statements.Add(node);
            }
        }
    }
}
