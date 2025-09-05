using Microsoft.VisualStudio.Shell.TableManager;

namespace SqlAnalyzerExtension
{
    class AnalyzerIssuesFactory : TableEntriesSnapshotFactoryBase
    {
        private readonly Analyzer analyzer;

        public AnalyzerIssuesSnapshot CurrentSnapshot { get; private set; }

        public AnalyzerIssuesFactory(Analyzer analyzer, AnalyzerIssuesSnapshot analyzerIssues)
        {
            this.analyzer = analyzer;

            this.CurrentSnapshot = analyzerIssues;
        }

        internal void UpdateErrors(AnalyzerIssuesSnapshot analyzerIssues)
        {
            this.CurrentSnapshot.NextSnapshot = analyzerIssues;
            this.CurrentSnapshot = analyzerIssues;
        }

        #region ITableEntriesSnapshotFactory members
        public override int CurrentVersionNumber
        {
            get
            {
                return this.CurrentSnapshot.VersionNumber;
            }
        }

        public override void Dispose()
        {
        }

        public override ITableEntriesSnapshot GetCurrentSnapshot()
        {
            return this.CurrentSnapshot;
        }

        public override ITableEntriesSnapshot GetSnapshot(int versionNumber)
        {
            // In theory the snapshot could change in the middle of the return statement so snap the snapshot just to be safe.
            var snapshot = this.CurrentSnapshot;
            return (versionNumber == snapshot.VersionNumber) ? snapshot : null;
        }
        #endregion
    }
}
