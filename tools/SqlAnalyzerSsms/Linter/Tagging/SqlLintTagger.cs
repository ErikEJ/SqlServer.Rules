using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using SqlAnalyzerSsms.Linter.Linting;

#pragma warning disable SA1309 // Field names should not begin with underscore - we prefer this for private fields
#pragma warning disable IDE1006 // Naming rule violation
namespace SqlAnalyzerSsms.Linter.Tagging
{
    /// <summary>
    /// Tagger that provides error tags for T-SQL lint violations.
    /// Uses shared SqlAnalysisCache to avoid duplicate parsing.
    /// </summary>
    public sealed class SqlLintTagger : ITagger<IErrorTag>, IDisposable
    {
        private readonly ITextBuffer _buffer;
        private readonly SqlAnalysisCache _analysisCache;
        private readonly string _filePath;
        private readonly object _lock = new();
        private readonly string _sqlVersion;
        private readonly string _rules;
        private readonly string _projectName;
        private ITextSnapshot _currentSnapshot;
        private List<LintResult> _currentResults;
        private bool _isDisposed;

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        public SqlLintTagger(ITextBuffer buffer, SqlAnalysisCache analysisCache, string sqlVersion, string rules, string projectName)
        {
            _buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
            _analysisCache = analysisCache ?? throw new ArgumentNullException(nameof(analysisCache));
            _currentSnapshot = buffer.CurrentSnapshot;
            _currentResults = [];
            _filePath = GetFilePath() ?? throw new InvalidOperationException("No file path available for current buffer.");
            _sqlVersion = sqlVersion;
            _rules = rules;
            _projectName = projectName;

            _buffer.Changed += OnBufferChanged;
            _analysisCache.AnalysisUpdated += OnAnalysisUpdated;

            // Initial analysis - immediate, no debounce for fast feedback on file open
            _analysisCache.AnalyzeImmediate(_buffer, _filePath, _sqlVersion, _rules, _projectName);
        }

        private void OnBufferChanged(object sender, TextContentChangedEventArgs e)
        {
            _currentSnapshot = e.After;

            // Debounced analysis during typing to reduce CPU usage
            _analysisCache.InvalidateAndAnalyze(_buffer, _filePath, _sqlVersion, _rules, _projectName);
        }

        private void OnAnalysisUpdated(object sender, AnalysisUpdatedEventArgs e)
        {
            if (e.Buffer != _buffer)
            {
                return;
            }

            ITextSnapshot snapshot = e.Snapshot;
            var results = e.Violations
                .Select(v => new LintResult(v, snapshot))
                .OrderBy(r => r.Start)
                .ToList();

            lock (_lock)
            {
                if (snapshot.Version.VersionNumber >= _currentSnapshot.Version.VersionNumber)
                {
                    _currentResults = results;

                    TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(
                        new SnapshotSpan(snapshot, 0, snapshot.Length)));
                }
            }
        }

        public IEnumerable<ITagSpan<IErrorTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count == 0)
            {
                yield break;
            }

            List<LintResult> results;
            lock (_lock)
            {
                results = [.. _currentResults];
            }

            ITextSnapshot currentSnapshot = spans[0].Snapshot;
            var queryStart = spans[0].Start.Position;
            var queryEnd = spans[spans.Count - 1].End.Position;

            foreach (LintResult result in results)
            {
                if (result.Start > queryEnd)
                {
                    break;
                }

                SnapshotSpan? span = result.GetTranslatedSpan(currentSnapshot);
                if (!span.HasValue)
                {
                    continue;
                }

                if (span.Value.End.Position < queryStart)
                {
                    continue;
                }

                if (IntersectsAnySpan(span.Value, spans))
                {
                    yield return new TagSpan<IErrorTag>(
                        span.Value,
                        new ErrorTag(GetErrorType(result.Severity)));
                }
            }
        }

        private static bool IntersectsAnySpan(SnapshotSpan target, NormalizedSnapshotSpanCollection spans)
        {
            for (var i = 0; i < spans.Count; i++)
            {
                SnapshotSpan candidate = spans[i];

                if (candidate.End.Position < target.Start.Position)
                {
                    continue;
                }

                if (candidate.Start.Position > target.End.Position)
                {
                    return false;
                }

                if (candidate.IntersectsWith(target))
                {
                    return true;
                }
            }

            return false;
        }

        private string GetErrorType(DiagnosticSeverity severity)
        {
            return severity switch
            {
                DiagnosticSeverity.Error => Microsoft.VisualStudio.Text.Adornments.PredefinedErrorTypeNames.SyntaxError,
                DiagnosticSeverity.Warning => Microsoft.VisualStudio.Text.Adornments.PredefinedErrorTypeNames.Warning,
                _ => Microsoft.VisualStudio.Text.Adornments.PredefinedErrorTypeNames.HintedSuggestion,
            };
        }

        private string? GetFilePath()
        {
            if (_buffer.Properties.TryGetProperty(typeof(ITextDocument), out ITextDocument document))
            {
                return document.FilePath;
            }

            return null;
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _buffer.Changed -= OnBufferChanged;
                _analysisCache.AnalysisUpdated -= OnAnalysisUpdated;
                _isDisposed = true;
            }
        }
    }
}
#pragma warning restore SA1309 // Field names should not begin with underscore
#pragma warning restore IDE1006 // Naming rule violation
