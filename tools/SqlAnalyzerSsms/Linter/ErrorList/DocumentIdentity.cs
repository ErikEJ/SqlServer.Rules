using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace SqlAnalyzerSsms.Linter.ErrorList
{
    internal static class DocumentIdentity
    {
        private const string DefaultDocumentName = "query.sql";

        public static (string FilePath, string DocumentName) Get(ITextView textView)
        {
            string? windowCaption = GetWindowCaption();
            string? virtualDocumentName = GetVirtualDocumentName(windowCaption);
            string filePath = GetFilePath(textView) ?? string.Empty;

            new InvalidOperationException(
                $"DocumentIdentity: filePath='{filePath}', windowCaption='{windowCaption}', virtualDocumentName='{virtualDocumentName}'")
                .Log();

            // Only prefer the virtual SSMS tab name when the underlying file is a temp file
            // (SSMS stores unsaved query windows in the user's %temp% folder).
            // For real files saved outside %temp%, preserve the actual file identity.
            bool useVirtualName = !string.IsNullOrWhiteSpace(virtualDocumentName)
                && (string.IsNullOrWhiteSpace(filePath) || IsInTempFolder(filePath));

            string documentName = useVirtualName
                ? virtualDocumentName!
                : (GetDocumentName(filePath) ?? windowCaption ?? string.Empty);

            if (string.IsNullOrWhiteSpace(documentName))
            {
                documentName = DefaultDocumentName;
            }

            if (string.IsNullOrWhiteSpace(filePath))
            {
                filePath = documentName;
            }
            else if (useVirtualName)
            {
                filePath = virtualDocumentName!;
            }

            return (filePath, documentName);
        }

        private static bool IsInTempFolder(string filePath)
        {
            string tempPath = Path.GetTempPath();
            return filePath.StartsWith(tempPath, StringComparison.OrdinalIgnoreCase);
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
            return null;
        }

        private static string? GetVirtualDocumentName(string? windowCaption)
        {
            if (string.IsNullOrWhiteSpace(windowCaption))
            {
                return null;
            }

            Match match = Regex.Match(windowCaption, @"^(?<name>SQLQuery\d+\.sql)");
            return match.Success ? match.Groups["name"].Value : null;
        }
    }
}
