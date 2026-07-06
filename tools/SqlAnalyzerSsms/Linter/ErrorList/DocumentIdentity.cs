using System;
using System.IO;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;

namespace SqlAnalyzerSsms.Linter.ErrorList
{
    internal static class DocumentIdentity
    {
        private const string DefaultDocumentName = "query.sql";

        public static (string FilePath, string DocumentName) Get(ITextView textView)
        {
            string? filePath = GetFilePath(textView);
            string? documentName = GetDocumentName(filePath);

            if (string.IsNullOrWhiteSpace(documentName))
            {
                documentName = GetWindowCaption();
            }

            if (string.IsNullOrWhiteSpace(documentName))
            {
                documentName = DefaultDocumentName;
            }

            if (string.IsNullOrWhiteSpace(filePath))
            {
                filePath = documentName;
            }

            return (filePath!, documentName);
        }

        private static string? GetFilePath(ITextView textView)
        {
            if (textView.TextBuffer.Properties.TryGetProperty(typeof(ITextDocument), out ITextDocument document))
            {
                return string.IsNullOrWhiteSpace(document.FilePath) ? null : document.FilePath;
            }

            return null;
        }

        private static string? GetDocumentName(string? filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return null;
            }

            string fileName = Path.GetFileName(filePath);
            return string.IsNullOrWhiteSpace(fileName) ? null : fileName;
        }

        private static string? GetWindowCaption()
        {
            try
            {
                var dte = Package.GetGlobalService(typeof(SDTE)) as DTE;
                if (dte?.ActiveWindow?.Caption is string caption)
                {
                    var trimmedCaption = caption.Trim();
                    if (!string.IsNullOrWhiteSpace(trimmedCaption))
                    {
                        var separatorIndex = trimmedCaption.IndexOf(" - ", StringComparison.Ordinal);
                        if (separatorIndex > 0)
                        {
                            trimmedCaption = trimmedCaption.Substring(0, separatorIndex);
                        }

                        return string.IsNullOrWhiteSpace(trimmedCaption) ? null : trimmedCaption;
                    }
                }
            }
            catch (Exception)
            {
                // Fall back to the default document name if we cannot resolve the active caption.
            }

            return null;
        }
    }
}
