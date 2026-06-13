using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Shell.TableManager;
using SqlAnalyzerSsms.Linter.Linting;

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

        public void UpdateErrors(string filePath, string projectName, IEnumerable<SqlAnalyzerDiagnosticInfo> violations)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            violations ??= [];

            var errors = violations.Select(v => new SqlLintError(v, filePath, projectName)).ToList();

            TableEntriesSnapshot? oldSnapshot = null;
            TableEntriesSnapshot? newSnapshot = null;

            lock (_snapshots)
            {
                _snapshots.TryGetValue(filePath, out oldSnapshot);
                if (oldSnapshot != null)
                {
                    _snapshots.Remove(filePath);
                }

                if (errors.Count > 0)
                {
                    newSnapshot = new TableEntriesSnapshot(filePath, errors);
                    _snapshots[filePath] = newSnapshot;
                }
            }

            // Notify sinks outside the lock to avoid potential deadlocks
            if (oldSnapshot != null)
            {
                NotifySinks(sink => sink.RemoveSnapshot(oldSnapshot));
            }

            if (newSnapshot != null)
            {
                NotifySinks(sink => sink.AddSnapshot(newSnapshot));
            }
        }

        public void ClearErrors(string filePath)
        {
            if (filePath == null)
            {
                return;
            }

            TableEntriesSnapshot? snapshot = null;

            lock (_snapshots)
            {
                if (_snapshots.TryGetValue(filePath, out snapshot))
                {
                    _snapshots.Remove(filePath);
                }
            }

            // Notify sinks outside the lock to avoid potential deadlocks
            if (snapshot != null)
            {
                NotifySinks(sink => sink.RemoveSnapshot(snapshot));
            }
        }

        public void ClearAllErrors()
        {
            List<TableEntriesSnapshot> snapshotsToRemove;

            lock (_snapshots)
            {
                snapshotsToRemove = new List<TableEntriesSnapshot>(_snapshots.Values);
                _snapshots.Clear();
            }

            // Notify sinks outside the lock to avoid potential deadlocks
            foreach (TableEntriesSnapshot snapshot in snapshotsToRemove)
            {
                NotifySinks(sink => sink.RemoveSnapshot(snapshot));
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
