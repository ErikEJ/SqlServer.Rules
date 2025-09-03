using Microsoft.VisualStudio.Shell.TableManager;
using System;

namespace SqlAnalyzerExtension
{
    /// <summary>
    /// Every consumer of data from an <see cref="ITableDataSource"/> provides an <see cref="ITableDataSink"/> to record the changes. We give the consumer
    /// an IDisposable (this object) that they hang on to as long as they are interested in our data (and they Dispose() of it when they are done).
    /// </summary>
    public class SinkManager : IDisposable
    {
        private readonly TaggerProvider _taggerProvider;
        private readonly ITableDataSink _sink;

        internal SinkManager(TaggerProvider taggerProvider, ITableDataSink sink)
        {
            _taggerProvider = taggerProvider;
            _sink = sink;

            taggerProvider.AddSinkManager(this);
        }

        public void Dispose()
        {
            // Called when the person who subscribed to the data source disposes of the cookie (== this object) they were given.
            _taggerProvider.RemoveSinkManager(this);
        }

        internal void AddAnalyzer(Analyzer analyzer)
        {
            _sink.AddFactory(analyzer.Factory);
        }

        internal void RemoveAnalyzer(Analyzer analyzer)
        {
            _sink.RemoveFactory(analyzer.Factory);
        }

        internal void UpdateSink()
        {
            _sink.FactorySnapshotChanged(null);
        }
    }
}
