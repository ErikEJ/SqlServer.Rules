using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
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
                if (Package.GetGlobalService(typeof(SComponentModel)) is not IComponentModel componentModel)
                {
                    Debug.WriteLine("Unable to resolve IComponentModel for SSMS window caption extraction.");
                    return null;
                }

                if (componentModel.GetService<IEditorAdapterFactoryService>() is not IEditorAdapterFactoryService editorFactory)
                {
                    Debug.WriteLine("Unable to resolve IEditorAdapterFactoryService for SSMS window caption extraction.");
                    return null;
                }

                if (editorFactory.GetViewAdapter(textView) is not IVsTextView viewAdapter)
                {
                    Debug.WriteLine("Unable to resolve the IVsTextView adapter for SSMS window caption extraction.");
                    return null;
                }

                int windowHandleResult = viewAdapter.GetWindowHandle(out IntPtr windowHandle);
                if (windowHandleResult != 0 || windowHandle == IntPtr.Zero)
                {
                    Debug.WriteLine($"GetWindowHandle failed for the current SSMS text view. HRESULT: 0x{windowHandleResult:X8}");
                    return null;
                }

                var title = new StringBuilder(MaxWindowTitleLength);
                if (GetWindowText(windowHandle, title, title.Capacity) <= 0)
                {
                    Debug.WriteLine($"Unable to retrieve the SSMS window title for the current text view. Win32 error: {Marshal.GetLastWin32Error()}");
                    return null;
                }

                var trimmedCaption = title.ToString().Trim();
                if (!string.IsNullOrWhiteSpace(trimmedCaption))
                {
                    // SSMS uses a tab caption format like "SqlQuery1.sql - " for virtual query tabs.
                    // The document name is the portion before the separator, which we keep for the error list.
                    // This assumes the standard SSMS caption format; localized or customized captions may use a different separator and would need a different parser.
                    // If that format changes, the fallback chain still uses the text document name first, then the window caption, then the default constant.
                    var separatorIndex = trimmedCaption.IndexOf(WindowTitleSeparator, StringComparison.Ordinal);
                    if (separatorIndex > 0)
                    {
                        trimmedCaption = trimmedCaption[..separatorIndex];
                    }

                    if (string.IsNullOrWhiteSpace(trimmedCaption))
                    {
                        return null;
                    }

                    return trimmedCaption;
                }
            }
            catch (COMException ex)
            {
                Debug.WriteLine(ex);
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine(ex);
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
