using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public class BooleanComparisonVisitor : BaseVisitor, IVisitor<BooleanComparisonExpression>
    {
        public IList<BooleanComparisonExpression> Statements { get; } = new List<BooleanComparisonExpression>();

        public int Count
        {
            get { return Statements.Count; }
        }

        public override void ExplicitVisit(BooleanComparisonExpression node)
        {
            Statements.Add(node);
        }
    }
}