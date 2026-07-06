using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Shell.TableManager;
using SqlAnalyzerSsms.Linter.Linting;

#pragma warning disable SA1309 // Field names should not begin with underscore - we prefer this for private fields
#pragma warning disable IDE1006 // Naming rule violation
namespace SqlAnalyzerSsms.Linter.ErrorList
{
    /// <summary>
    /// Table data source for the Error List window.
    /// </summary>
    [Export(typeof(SqlLintTableDataSource))]
    public class SqlLintTableDataSource : ITableDataSource
    {
        private static SqlLintTableDataSource _instance;

        public static SqlLintTableDataSource Instance => _instance;

        private readonly List<SinkManager> _managers = [];
        private readonly Dictionary<string, TableEntriesSnapshot> _snapshots =
            new(StringComparer.OrdinalIgnoreCase);

        public string SourceTypeIdentifier => StandardTableDataSources.ErrorTableDataSource;

        public string Identifier => "SqlProjects";

        public string DisplayName => Vsix.Name;

        [ImportingConstructor]
        public SqlLintTableDataSource([Import] ITableManagerProvider tableManagerProvider)
        {
            _instance = this;

            ITableManager tableManager = tableManagerProvider.GetTableManager(StandardTables.ErrorsTable);
            tableManager.AddSource(
                this,
                StandardTableColumnDefinitions.Column,
                StandardTableColumnDefinitions.DocumentName,
                StandardTableColumnDefinitions.ErrorCode,
                StandardTableColumnDefinitions.ErrorSeverity,
                StandardTableColumnDefinitions.Line,
                StandardTableColumnDefinitions.Text,
                StandardTableColumnDefinitions.ProjectName);
        }

        public IDisposable Subscribe(ITableDataSink sink)
        {
            var manager = new SinkManager(this, sink);

            lock (_managers)
            {
                _managers.Add(manager);
            }

            // Send existing snapshots to new sink
            lock (_snapshots)
            {
                foreach (TableEntriesSnapshot snapshot in _snapshots.Values)
                {
                    sink.AddSnapshot(snapshot);
                }
            }

            return manager;
        }

        public void UpdateErrors(string filePath, string documentName, string projectName, IEnumerable<SqlAnalyzerDiagnosticInfo> violations)
        {
            string effectivePath = string.IsNullOrWhiteSpace(filePath) ? documentName : filePath;
            if (string.IsNullOrWhiteSpace(effectivePath))
            {
                return;
            }

            violations ??= [];

            var errors = violations.Select(v => new SqlLintError(v, effectivePath, documentName, projectName)).ToList();

            lock (_snapshots)
            {
                if (_snapshots.TryGetValue(effectivePath, out TableEntriesSnapshot oldSnapshot))
                {
                    _snapshots.Remove(effectivePath);
                    NotifySinks(sink => sink.RemoveSnapshot(oldSnapshot));
                }

                if (errors.Count > 0)
                {
                    var snapshot = new TableEntriesSnapshot(effectivePath, errors);
                    _snapshots[effectivePath] = snapshot;
                    NotifySinks(sink => sink.AddSnapshot(snapshot));
                }
            }
        }

        public void ClearErrors(string filePath)
        {
            if (filePath == null)
            {
                return;
            }

            lock (_snapshots)
            {
                if (_snapshots.TryGetValue(filePath, out TableEntriesSnapshot snapshot))
                {
                    _snapshots.Remove(filePath);
                    NotifySinks(sink => sink.RemoveSnapshot(snapshot));
                }
            }
        }

        public void ClearAllErrors()
        {
            lock (_snapshots)
            {
                foreach (TableEntriesSnapshot snapshot in _snapshots.Values)
                {
                    NotifySinks(sink => sink.RemoveSnapshot(snapshot));
                }

                _snapshots.Clear();
            }
        }

        private void NotifySinks(Action<ITableDataSink> action)
        {
            lock (_managers)
            {
                foreach (SinkManager manager in _managers)
                {
                    action(manager.Sink);
                }
            }
        }

        internal void RemoveSinkManager(SinkManager manager)
        {
            lock (_managers)
            {
                _managers.Remove(manager);
            }
        }
    }
}
#pragma warning restore SA1309 // Field names should not begin with underscore
#pragma warning restore IDE1006 // Naming rule violation
