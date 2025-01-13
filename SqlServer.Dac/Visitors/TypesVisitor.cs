using System;
using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public class TypesVisitor : BaseVisitor, IVisitor<TSqlFragment>
    {
        private readonly List<Type> _types = new List<Type>();
        public IList<TSqlFragment> Statements { get; } = new List<TSqlFragment>();
        public int Count { get { return Statements.Count; } }
        public TypesVisitor(params Type[] typesToLookFor)
        {
            if (typesToLookFor.Length == 0) { throw new ArgumentNullException(nameof(typesToLookFor)); }
            _types = new List<Type>(typesToLookFor);
        }

        public override void Visit(TSqlFragment fragment)
        {
            if (_types.Contains(fragment.GetType()))
            {
                Statements.Add(fragment);
            }
        }
    }
}
