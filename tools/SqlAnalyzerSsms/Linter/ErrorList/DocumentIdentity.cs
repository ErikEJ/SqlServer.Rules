using System;
using System.Collections.Generic;
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

                // GetWindowHandle() returns the inner edit-control HWND whose window text is the
                // temp file name (e.g. "v5lioamz..sql").  The SSMS tab title that carries the
                // virtual document name ("SQLQuery6.sql - ...") lives on the MDI child window
                // further up the parent chain.  Walk upward to find the right caption.
                return GetCaptionFromWindowChain(windowHandle);
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
        /// Walks up the Win32 parent-window chain from <paramref name="startHandle"/> and returns
        /// the first window title that contains the virtual document name pattern.  If none match,
        /// returns the first non-empty title found (preserving previous behaviour) and logs a
        /// diagnostic message with all collected captions to aid troubleshooting.
        /// </summary>
        private static string? GetCaptionFromWindowChain(IntPtr startHandle)
        {
            const int maxDepth = 10;
            var captions = new List<string>();
            var hWnd = startHandle;

            for (int i = 0; i < maxDepth && hWnd != IntPtr.Zero; i++)
            {
                var titleBuilder = new StringBuilder(MaxWindowTitleLength);
                if (GetWindowText(hWnd, titleBuilder, titleBuilder.Capacity) > 0)
                {
                    captions.Add(titleBuilder.ToString().Trim());
                }

                hWnd = GetParent(hWnd);
            }

            // Prefer the first caption that yields a virtual document name
            foreach (string caption in captions)
            {
                if (GetVirtualDocumentName(caption) != null)
                {
                    return caption;
                }
            }

            // No virtual name found — log all captions for diagnostics and fall back to
            // the first non-empty one (preserves previous behaviour for saved files).
            if (captions.Count > 0)
            {
                new InvalidOperationException(
                    $"DocumentIdentity: could not resolve virtual document name. " +
                    $"Window captions tried ({captions.Count}): [{string.Join("] [", captions)}]")
                    .Log();
                return captions[0];
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

        /// <summary>
        /// Returns the handle of the parent window, or <see cref="IntPtr.Zero"/> if none.
        /// </summary>
        [DllImport("user32.dll", ExactSpelling = true)]
        private static extern IntPtr GetParent(IntPtr hWnd);
    }
}
