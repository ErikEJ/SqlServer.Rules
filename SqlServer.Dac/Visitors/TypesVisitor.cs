using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlServer.Dac.Visitors
{
    public class TypesVisitor : BaseVisitor, IVisitor<TSqlFragment>
    {
        private readonly IList<Type> _types = new List<Type>();
        public IList<TSqlFragment> Statements { get; } = new List<TSqlFragment>();
        public int Count { get { return Statements.Count; } }
        public TypesVisitor(params Type[] typesToLookFor)
        {
            if (!typesToLookFor.Any()) { throw new ArgumentNullException(nameof(typesToLookFor)); }
            _types = new List<Type>(typesToLookFor);
        }

        public override void Visit(TSqlFragment node)
        {
            if (_types.Contains(node.GetType()))
            {
                Statements.Add(node);
            }
        }
    }
}
