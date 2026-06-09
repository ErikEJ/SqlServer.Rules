using System.Collections.Generic;
using Microsoft.VisualStudio.Text;

namespace SqlAnalyzerSsms.Linter.Linting
{
    /// <summary>
    /// Event args for analysis completion.
    /// </summary>
    public class AnalysisUpdatedEventArgs(
        ITextBuffer buffer,
        ITextSnapshot snapshot,
        IReadOnlyList<SqlAnalyzerDiagnosticInfo> violations,
        string filePath,
        string projectName) : EventArgs
    {
        public ITextBuffer Buffer { get; } = buffer;

        public ITextSnapshot Snapshot { get; } = snapshot;

        public IReadOnlyList<SqlAnalyzerDiagnosticInfo> Violations { get; } = violations;

        public string FilePath { get; } = filePath;

        public string ProjectName { get; } = projectName;
    }
}
