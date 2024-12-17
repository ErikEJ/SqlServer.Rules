using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;

namespace SqlServer.Dac.Visitors
{
    public interface IVisitor<T> : IBaseVisitor where T : TSqlFragment
    {
        IList<T> Statements { get; }
    }
}
