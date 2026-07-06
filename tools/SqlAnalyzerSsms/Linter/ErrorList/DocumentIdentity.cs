using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace SqlAnalyzerSsms.Linter.ErrorList
{
    internal static class DocumentIdentity
    {
        private const string DefaultDocumentName = "query.sql";
        private const int MaxWindowTitleLength = 1024;

        public static (string FilePath, string DocumentName) Get(ITextView textView)
        {
            string? windowCaption = GetWindowCaption(textView);
            string? virtualDocumentName = GetVirtualDocumentName(windowCaption);
            string filePath = GetFilePath(textView) ?? string.Empty;

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

        private static string? GetWindowCaption(ITextView textView)
        {
            try
            {
                if (!textView.Properties.TryGetProperty(typeof(IVsTextView), out IVsTextView vsTextView) || vsTextView == null)
                {
                    return null;
                }

                var windowHandle = vsTextView.GetWindowHandle();
                if (windowHandle == IntPtr.Zero)
                {
                    return null;
                }

                var titleBuilder = new StringBuilder(MaxWindowTitleLength);
                if (GetWindowText(windowHandle, titleBuilder, titleBuilder.Capacity) <= 0)
                {
                    return null;
                }

                return titleBuilder.ToString().Trim();
            }
            catch (COMException ex)
            {
                ex.Log();
            }
            catch (InvalidOperationException ex)
            {
                ex.Log();
            }

            return null;
        }

        private static string? GetVirtualDocumentName(string? windowCaption)
        {
            if (string.IsNullOrWhiteSpace(windowCaption))
            {
                return null;
            }

            Match match = Regex.Match(windowCaption, @"^(?<name>SQLQuery\d+\.sql)\s+-");
            return match.Success ? match.Groups["name"].Value : null;
        }

        /// <summary>
        /// Retrieves the text of the specified window title bar.
        /// </summary>
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
    }
}
