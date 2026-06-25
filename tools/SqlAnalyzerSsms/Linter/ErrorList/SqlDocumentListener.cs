using System.ComponentModel.Composition;
using System.IO;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
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
        internal SqlLintTableDataSource TableDataSource { get; set; } = null!;

        [Import]
        internal SqlAnalysisCache AnalysisCache { get; set; } = null!;

        public void TextViewCreated(ITextView textView)
        {
            var filePath = GetFilePath(textView);
            if (filePath == null)
            {
                return;
            }

            var documentName = GetDocumentName(textView, filePath);

#pragma warning disable CA2000 // Dispose objects before losing scope
            var handler = new DocumentHandler(textView, TableDataSource, AnalysisCache, filePath, documentName);
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

        private static string GetDocumentName(ITextView textView, string filePath)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (textView.Properties.TryGetProperty(typeof(IVsWindowFrame), out IVsWindowFrame frame)
                && frame.GetProperty((int)__VSFPROPID.VSFPROPID_Caption, out object captionObject) >= 0
                && captionObject is string caption
                && !string.IsNullOrWhiteSpace(caption))
            {
                return ExtractSqlFileName(caption);
            }

            return Path.GetFileName(filePath);
        }

        private static string ExtractSqlFileName(string caption)
        {
            const string sqlSuffix = ".sql ";
            var index = caption.IndexOf(sqlSuffix, System.StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                return caption.Substring(0, index + 4); // include ".sql", exclude trailing space
            }

            return caption;
        }
    }
}
