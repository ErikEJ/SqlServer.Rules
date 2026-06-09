using System.Collections.Generic;

namespace SqlAnalyzerSsms.Linter.Linting
{
    /// <summary>
    /// Cached analysis result for a text buffer.
    /// </summary>
    internal sealed class CachedAnalysisResult(int snapshotVersion, IReadOnlyList<SqlAnalyzerDiagnosticInfo> violations)
    {
        public int SnapshotVersion { get; } = snapshotVersion;

        public IReadOnlyList<SqlAnalyzerDiagnosticInfo> Violations { get; } = violations;
    }
}
