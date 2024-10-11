﻿using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;

namespace SqlServer.Dac.Visitors
{
    public class CloseCursorVisitor : BaseVisitor, IVisitor<CloseCursorStatement>
    {
        public IList<CloseCursorStatement> Statements { get; } = new List<CloseCursorStatement>();
        public int Count { get { return this.Statements.Count; } }
        public override void ExplicitVisit(CloseCursorStatement node)
        {
            Statements.Add(node);
        }
    }
}