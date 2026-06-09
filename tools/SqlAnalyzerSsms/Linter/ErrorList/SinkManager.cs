using Microsoft.VisualStudio.Shell.TableManager;
using System;

namespace SqlAnalyzerSsms.Linter.ErrorList
{
    /// <summary>
    /// Manages subscription to the table data sink.
    /// </summary>
    internal sealed class SinkManager(SqlLintTableDataSource source, ITableDataSink sink) : IDisposable
    {
        public ITableDataSink Sink { get; } = sink;

        public void Dispose()
        {
            source.RemoveSinkManager(this);
        }
    }
}
