using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using SqlAnalyzerSsms.Linter.Linting;

namespace SqlAnalyzerSsms.Linter.ErrorList
{
    /// <summary>
    /// Listens for document changes and updates the error list.
    /// Uses shared SqlAnalysisCache to avoid duplicate parsing.
    /// </summary>
    [Export(typeof(ITextViewCreationListener))]
    [ContentType("SQL")]
    [ContentType("SQL Server Tools")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    public class SqlDocumentListener : ITextViewCreationListener
    {
        [Import]
        internal SqlLintTableDataSource TableDataSource { get; set; }

        [Import]
        internal SqlAnalysisCache AnalysisCache { get; set; }

        public void TextViewCreated(ITextView textView)
        {
            var filePath = GetFilePath(textView);
            if (filePath == null)
            {
                return;
            }

#pragma warning disable CA2000 // Dispose objects before losing scope
            var handler = new DocumentHandler(textView, TableDataSource, AnalysisCache, filePath);
#pragma warning restore CA2000 // Dispose objects before losing scope
            textView.Closed += (s, e) => handler.Dispose();
        }

        private static string? GetFilePath(ITextView textView)
        {
            if (textView.TextBuffer.Properties.TryGetProperty(typeof(ITextDocument), out ITextDocument document))
            {
                return document.FilePath;
            }

            return null;
        }
    }
}
