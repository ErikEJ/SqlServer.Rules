using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public class GlobalVariableExpressionVisitor : BaseVisitor, IVisitor<GlobalVariableExpression>
    {
        private readonly IList<string> _variableNames = new List<string>();
        public IList<GlobalVariableExpression> Statements { get; } = new List<GlobalVariableExpression>();
        public int Count { get { return Statements.Count; } }
        public GlobalVariableExpressionVisitor()
        {
        }

        public GlobalVariableExpressionVisitor(params string[] variableNames)
        {
            _variableNames = variableNames.ToList();
        }

        public override void Visit(GlobalVariableExpression node)
        {
            if (!_variableNames.Any() || _variableNames.FirstOrDefault(p => _comparer.Equals(node.Name, p)) != null)
            {
                Statements.Add(node);
            }
        }
    }
}