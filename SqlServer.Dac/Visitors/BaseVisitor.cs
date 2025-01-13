using System;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac.Visitors
{
    public abstract class BaseVisitor : TSqlFragmentVisitor
    {
#pragma warning disable CA1051 // Do not declare visible instance fields
        protected readonly StringComparer _comparer = StringComparer.InvariantCultureIgnoreCase;
#pragma warning restore CA1051 // Do not declare visible instance fields
    }
}
