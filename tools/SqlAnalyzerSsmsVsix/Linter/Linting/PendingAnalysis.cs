using System.Threading;

namespace SqlAnalyzerSsms.Linter.Linting
{
    internal sealed class PendingAnalysis(CancellationTokenSource cancellationTokenSource, int snapshotVersion)
    {
        public CancellationTokenSource CancellationTokenSource { get; } = cancellationTokenSource;

        public int SnapshotVersion { get; } = snapshotVersion;
    }
}
