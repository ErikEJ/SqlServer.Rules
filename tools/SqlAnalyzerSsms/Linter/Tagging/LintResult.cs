using Microsoft.VisualStudio.Text;
using SqlAnalyzerSsms.Linter.Linting;
using System;

namespace SqlAnalyzerSsms.Linter.Tagging
{
    /// <summary>
    /// Represents a lint result with tracking span support.
    /// </summary>
    public class LintResult
    {
        private readonly ITrackingSpan trackingSpan;

        public string RuleId { get; }

        public string Message { get; }

        public string? DocumentationLink { get; }

        public DiagnosticSeverity Severity { get; }

        public int Start { get; }

        public LintResult(SqlAnalyzerDiagnosticInfo violation, ITextSnapshot snapshot)
        {
            if (violation == null)
            {
                throw new ArgumentNullException(nameof(violation));
            }

            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            RuleId = violation.ErrorCode;
            Message = violation.Message;
            DocumentationLink = violation.HelpLink?.ToString();
            Severity = DiagnosticSeverity.Warning;

            // Calculate span from line/column
            ITextSnapshotLine line = snapshot.GetLineFromLineNumber(Math.Min(violation.Range.StartLine, snapshot.LineCount - 1));
            var startIndex = line.Start.Position + Math.Min(violation.Range.StartColumn, line.Length);
            var endIndex = line.Start.Position + Math.Min(violation.Range.EndColumn, line.Length);

            if (endIndex <= startIndex)
            {
                if (startIndex < line.End.Position && IsWordChar(snapshot[startIndex]))
                {
                    endIndex = startIndex + 1;
                    while (endIndex < line.End.Position && IsWordChar(snapshot[endIndex]))
                    {
                        endIndex++;
                    }
                }
                else
                {
                    endIndex = Math.Min(startIndex + 1, line.End.Position);
                }
            }

            var span = new Span(startIndex, Math.Max(1, endIndex - startIndex));
            Start = span.Start;
            trackingSpan = snapshot.CreateTrackingSpan(span, SpanTrackingMode.EdgeExclusive);
        }

        public SnapshotSpan? GetTranslatedSpan(ITextSnapshot snapshot)
        {
            try
            {
                return trackingSpan.GetSpan(snapshot);
            }
            catch
            {
                return null;
            }
        }

        private static bool IsWordChar(char c) => char.IsLetterOrDigit(c) || c == '_';
    }
}
