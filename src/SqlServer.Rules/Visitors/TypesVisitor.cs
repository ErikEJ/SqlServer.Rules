using System;
using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public class TypesVisitor : BaseVisitor, IVisitor<TSqlFragment>
    {
        private readonly HashSet<Type> types;

        public IList<TSqlFragment> Statements { get; } = new List<TSqlFragment>();

        public int Count
        {
            get { return Statements.Count; }
        }

        public TypesVisitor(params Type[] typesToLookFor)
        {
            types = typesToLookFor is { Length: > 0 }
                ? new HashSet<Type>(typesToLookFor)
                : new HashSet<Type>();
        }

        public override void Visit(TSqlFragment fragment)
        {
            if (types.Contains(fragment.GetType()))
            {
                Statements.Add(fragment);
            }
        }
    }
}
