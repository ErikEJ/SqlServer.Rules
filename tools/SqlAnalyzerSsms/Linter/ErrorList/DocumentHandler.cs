using System;
using Microsoft.VisualStudio.Text.Editor;
using SqlAnalyzerSsms.Linter.Linting;

#pragma warning disable SA1309 // Field names should not begin with underscore - we prefer this for private fields
#pragma warning disable IDE1006 // Naming rule violation
namespace SqlAnalyzerSsms.Linter.ErrorList
{
    /// <summary>
    /// Handles document events for a specific text view.
    /// Listens to shared analysis cache for results.
    /// Note: Debouncing is handled by SqlAnalysisCache, not here.
    /// </summary>
    internal sealed class DocumentHandler : IDisposable
    {
        private readonly ITextView _textView;
        private readonly SqlLintTableDataSource _tableDataSource;
        private readonly SqlAnalysisCache _analysisCache;
        private readonly string _filePath;
        private readonly string _documentName;
        private bool _disposed;

        public DocumentHandler(
            ITextView textView,
            SqlLintTableDataSource tableDataSource,
            SqlAnalysisCache analysisCache,
            string filePath,
            string documentName)
        {
            _textView = textView;
            _tableDataSource = tableDataSource;
            _analysisCache = analysisCache;
            _filePath = filePath;
            _documentName = documentName;

            // Only listen for analysis results — the tagger owns triggering analysis
            // (on buffer changes, option saves, and initial file open).
            _analysisCache.AnalysisUpdated += OnAnalysisUpdated;
        }

        private void OnAnalysisUpdated(object sender, AnalysisUpdatedEventArgs e)
        {
            if (e.Buffer != _textView.TextBuffer)
            {
                return;
            }

            // Update error list with new results
            _tableDataSource?.UpdateErrors(e.FilePath, _documentName, e.ProjectName, e.Violations);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _analysisCache.AnalysisUpdated -= OnAnalysisUpdated;
                _tableDataSource?.ClearErrors(_filePath);
            }
        }
    }
}
#pragma warning restore SA1309 // Field names should not begin with underscore
#pragma warning restore IDE1006 // Naming rule violation
