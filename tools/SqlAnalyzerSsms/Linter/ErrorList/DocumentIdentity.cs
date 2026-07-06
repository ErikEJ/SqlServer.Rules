using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
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
        private const string WindowTitleSeparator = " - ";
        private const string SqlExtension = ".sql";

        public static (string FilePath, string DocumentName) Get(ITextView textView)
        {
            string filePath = GetFilePath(textView) ?? string.Empty;
            string documentName = GetDocumentName(filePath) ?? string.Empty;

            if (string.IsNullOrWhiteSpace(documentName))
            {
                documentName = GetWindowCaption(textView) ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(documentName))
            {
                documentName = DefaultDocumentName;
            }

            if (string.IsNullOrWhiteSpace(filePath))
            {
                filePath = documentName;
            }

            return (filePath, documentName);
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

                var title = titleBuilder.ToString().Trim();

                // SSMS virtual query tabs use a window title format like
                // "SQLQuery1.sql - <server>.<db> - Microsoft SQL Server Management Studio".
                // Look for ".sql - " to identify a virtual document and extract the name.
                string sqlMarker = SqlExtension + WindowTitleSeparator;
                var markerIndex = title.IndexOf(sqlMarker, StringComparison.Ordinal);
                if (markerIndex >= 0)
                {
                    return title.Substring(0, markerIndex + SqlExtension.Length);
                }
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

        /// <summary>
        /// Retrieves the text of the specified window title bar.
        /// </summary>
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
    }
}
