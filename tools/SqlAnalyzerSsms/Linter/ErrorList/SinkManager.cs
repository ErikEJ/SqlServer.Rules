using System;
using Microsoft.VisualStudio.Shell.TableManager;

namespace SqlAnalyzerSsms.Linter.ErrorList
{
    internal sealed class SinkManager(SqlLintTableDataSource source, ITableDataSink sink) : IDisposable
    {
        public ITableDataSink Sink { get; } = sink;

        public void Dispose()
        {
            source.RemoveSinkManager(this);
        }
    }
}
