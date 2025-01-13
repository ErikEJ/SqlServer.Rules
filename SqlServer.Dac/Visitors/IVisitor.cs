using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public interface IVisitor<T> : IBaseVisitor where T : TSqlFragment
    {
        IList<T> Statements { get; }
    }
}
