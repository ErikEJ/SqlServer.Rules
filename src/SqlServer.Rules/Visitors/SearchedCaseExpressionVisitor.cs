using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public class SearchedCaseExpressionVisitor : BaseVisitor, IVisitor<SearchedCaseExpression>
    {
        public IList<SearchedCaseExpression> Statements { get; } = new List<SearchedCaseExpression>();

        public int Count => Statements.Count;

        public override void Visit(SearchedCaseExpression node)
        {
            Statements.Add(node);
        }
    }
}
