using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft;
using Microsoft.VisualStudio.Extensibility.Editor;
using Microsoft.VisualStudio.Extensibility.Languages;
using Microsoft.VisualStudio.RpcContracts.DiagnosticManagement;
using Microsoft.VisualStudio.Threading;

namespace SqlAnalyzer;

#pragma warning disable VSEXTPREVIEW_SETTINGS // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

/// <summary>
/// Helper class for running analyzer on a string or file.
/// </summary>
internal class AnalyzerUtilities
{
    private string tempPath = Path.Combine(Path.GetTempPath(), "tsqlanalyzerscratch.sql");

    /// <summary>
    /// Runs SQL analyzer on a file uri and returns diagnostic entries.
    /// </summary>
    /// <param name="fileUri">File uri to run SQL analyzer on.</param>
    /// <param name="rules">Rules to apply for the analyzer.</param>
    /// <param name="sqlVersion">SQL version to use for analysis.</param>
    /// <param name="cancellationToken">Cancellation token to monitor.</param>
    /// <returns>an enumeration of <see cref="DocumentDiagnostic"/> entries for warnings in the SQL file.</returns>
    public async Task<IEnumerable<DocumentDiagnostic>> RunAnalyzerOnFileAsync(Uri fileUri, string? rules, string? sqlVersion, CancellationToken cancellationToken)
    {
        using var analyzer = new Process();
        var lineQueue = new AsyncQueue<string>();

        StartAnalyzerProcess(analyzer, lineQueue, fileUri.LocalPath, rules, sqlVersion);

        var sqlDiagnostics = await ProcessAnalyzerQueueAsync(lineQueue);
        return CreateDocumentDiagnosticsForClosedDocument(fileUri, sqlDiagnostics);
    }

    /// <summary>
    /// Runs SQL analyzer on a given text document and returns diagnostic entries.
    /// </summary>
    /// <param name="textDocument">Document to run SQL analyzer on.</param>
    /// <param name="rules">Rules to apply for the analyzer.</param>
    /// <param name="sqlVersion">SQL version to use for analysis.</param>
    /// <param name="cancellationToken">Cancellation token to monitor.</param>
    /// <returns>an enumeration of <see cref="DocumentDiagnostic"/> entries for warnings in the SQL file.</returns>
    public async Task<IEnumerable<DocumentDiagnostic>> RunAnalyzerOnDocumentAsync(ITextDocumentSnapshot textDocument, string? rules, string? sqlVersion, CancellationToken cancellationToken)
    {
        using var analyzer = new Process();
        var lineQueue = new AsyncQueue<string>();

        if (textDocument.Length > 8192)
        {
            // SQL analyzer has issues processing very large files.
            return Array.Empty<DocumentDiagnostic>();
        }

        var content = textDocument.Text.CopyToString();

        await File.WriteAllTextAsync(tempPath, content, Encoding.UTF8, cancellationToken);

        StartAnalyzerProcess(analyzer, lineQueue, tempPath, rules, sqlVersion);

        var sqlDiagnostics = await ProcessAnalyzerQueueAsync(lineQueue);
        return CreateDocumentDiagnosticsForOpenDocument(textDocument, sqlDiagnostics);
    }

    private static void StartAnalyzerProcess(Process analyzer, AsyncQueue<string> lineQueue, string path, string? rules, string? sqlVersion)
    {
        bool useDnx = IsVisualStudioVersion18OrLater();
        string fileName;
        string args;

        if (useDnx)
        {
            // Use dnx syntax for VS 2026 (version 18) or later
            fileName = "dnx";
            args = "ErikEJ.DacFX.TSQLAnalyzer.Cli --yes -- -n -i" +
                $" \"{path}\"";

            if (!string.IsNullOrWhiteSpace(rules))
            {
                args = args + $" -r Rules:{rules}";
            }

            if (!string.IsNullOrWhiteSpace(sqlVersion))
            {
                args = args + $" -s {sqlVersion}";
            }
        }
        else
        {
            // Use tsqlanalyze command for older VS versions
            fileName = "cmd.exe";
            args = "/c \"tsqlanalyze -n -i" +
                $" \"{path}\"\"";

            if (!string.IsNullOrWhiteSpace(rules))
            {
                args = args + $" -r Rules:{rules}";
            }

            if (!string.IsNullOrWhiteSpace(sqlVersion))
            {
                args = args + $" -s {sqlVersion}";
            }
        }

        analyzer.StartInfo = new ProcessStartInfo()
        {
            FileName = fileName,
            Arguments = args,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        analyzer.EnableRaisingEvents = true;
        analyzer.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
        {
            if (e.Data is not null)
            {
                lineQueue.Enqueue(e.Data);
            }
            else
            {
                lineQueue.Complete();
            }
        });

        try
        {
            analyzer.Start();
            analyzer.BeginOutputReadLine();
        }
        catch (Win32Exception ex)
        {
            throw new InvalidOperationException(message: ex.Message, innerException: ex);
        }
    }

    /// <summary>
    /// Determines if the current Visual Studio host is version 18 or later.
    /// </summary>
    /// <returns>True if running in VS 2026 (version 18) or later, false otherwise.</returns>
    private static bool IsVisualStudioVersion18OrLater()
    {
        try
        {
            using var process = Process.GetCurrentProcess();
            if (process.MainModule?.FileName != null)
            {
                var versionInfo = FileVersionInfo.GetVersionInfo(process.MainModule.FileName);
                return versionInfo.FileMajorPart >= 18;
            }
        }
        catch (Exception)
        {
            // If we can't determine the version (due to security restrictions, process access issues, etc.),
            // fall back to the old behavior (using tsqlanalyze command).
            // This ensures the extension continues to work even if version detection fails.
        }

        return false;
    }

    private static IEnumerable<DocumentDiagnostic> CreateDocumentDiagnosticsForOpenDocument(ITextDocumentSnapshot document, IEnumerable<SqlAnalyzerDiagnosticInfo> diagnostics)
    {
        foreach (var diagnostic in diagnostics)
        {
            var startindex = document.Lines[diagnostic.Range.StartLine].Text.Start.Offset;
            if (diagnostic.Range.StartColumn >= 0)
            {
                startindex += diagnostic.Range.StartColumn;
            }

            var endIndex = document.Lines[diagnostic.Range.EndLine].Text.Start.Offset;
            if (diagnostic.Range.EndColumn >= 0)
            {
                endIndex += diagnostic.Range.EndColumn;
            }

            yield return new DocumentDiagnostic(new TextRange(document, startindex, endIndex - startindex), diagnostic.Message)
            {
                ErrorCode = diagnostic.ErrorCode,
                Severity = DiagnosticSeverity.Warning,
                ProviderName = Strings.AnalyzerWindowName,
                HelpLink = diagnostic.HelpLink?.ToString(),
            };
        }
    }

    private static IEnumerable<DocumentDiagnostic> CreateDocumentDiagnosticsForClosedDocument(Uri fileUri, IEnumerable<SqlAnalyzerDiagnosticInfo> diagnostics)
    {
        foreach (var diagnostic in diagnostics)
        {
            yield return new DocumentDiagnostic(fileUri, diagnostic.Range, diagnostic.Message)
            {
                ErrorCode = diagnostic.ErrorCode,
                Severity = DiagnosticSeverity.Warning,
                ProviderName = Strings.AnalyzerWindowName,
                HelpLink = diagnostic.HelpLink?.ToString(),
            };
        }
    }

    private static async Task<IEnumerable<SqlAnalyzerDiagnosticInfo>> ProcessAnalyzerQueueAsync(AsyncQueue<string> lineQueue)
    {
        Requires.NotNull(lineQueue, nameof(lineQueue));

        List<SqlAnalyzerDiagnosticInfo> diagnostics = new List<SqlAnalyzerDiagnosticInfo>();

        while (!(lineQueue.IsCompleted && lineQueue.IsEmpty))
        {
            string? line;
            try
            {
                line = await lineQueue.DequeueAsync();
            }
            catch (OperationCanceledException)
            {
                break;
            }

            var diagnostic = line is not null ? GetDiagnosticFromAnalyzerOutput(line) : null;
            if (diagnostic is not null)
            {
                diagnostics.Add(diagnostic);
            }
            else
            {
                // Something went wrong so break and return the current set.
                break;
            }
        }

        return diagnostics;
    }

    private static SqlAnalyzerDiagnosticInfo? GetDiagnosticFromAnalyzerOutput(string outputLine)
    {
        Requires.NotNull(outputLine, nameof(outputLine));

        var parsed = ParseSqlAnalyzerOutput(outputLine);

        if (parsed is null)
        {
            // The output line does not match the expected format.
            return null;
        }

        return new SqlAnalyzerDiagnosticInfo(
            range: new Microsoft.VisualStudio.RpcContracts.Utilities.Range(startLine: parsed.Value.Line - 1, startColumn: parsed.Value.Column - 1),
            message: parsed.Value.Description,
            errorCode: parsed.Value.RuleId,
            helpLink: !string.IsNullOrEmpty(parsed.Value.Url) ? new Uri(parsed.Value.Url) : null);
    }

    /// <summary>
    /// Parses a SQL analyzer output line and extracts its components.
    /// </summary>
    /// <param name="outputLine">The output line to parse</param>
    /// <returns>A tuple containing the parsed components, or null if parsing fails</returns>
    public static (string Filename, int Line, int Column, string RuleId, string Description, string Url)? ParseSqlAnalyzerOutput(string outputLine)
    {
        if (string.IsNullOrWhiteSpace(outputLine))
        {
            return null;
        }

        var parts = outputLine.Split(": ", 3);

        if (parts.Length < 3)
        {
            return null;
        }

        // C:\Users\ErikEjlskovJensen(De\AppData\Local\Temp\tsqlanalyzerscratch.sql(6,9): Smells.SML005 : Avoid use of 'Select *'. (https://github.com/ErikEJ/SqlServer.Rules/blob/master/docs/CodeSmells/SML005.md)
        var fileAndPosition = parts[0].Trim();
        var lineColumnStart = fileAndPosition.LastIndexOf('(');
        var lineColumnEnd = fileAndPosition.LastIndexOf(')');
        if (lineColumnStart < 0 || lineColumnEnd < 0 || lineColumnEnd <= lineColumnStart)
        {
            return null;
        }

        var lineColumn = fileAndPosition.Substring(lineColumnStart + 1, lineColumnEnd - lineColumnStart - 1);
        var lineColumnParts = lineColumn.Split(',');
        if (lineColumnParts.Length != 2 ||
            !int.TryParse(lineColumnParts[0], out int line) ||
            !int.TryParse(lineColumnParts[1], out int column))
        {
            return null;
        }

        fileAndPosition = fileAndPosition.Substring(0, lineColumnStart);
        var ruleId = parts[1].Trim();
        var description = parts[2].Trim();
        var urlStartIndex = description.IndexOf(" (https", StringComparison.OrdinalIgnoreCase);
        var url = urlStartIndex >= 0 ? description.Substring(urlStartIndex + 2, description.Length - urlStartIndex - 3) : string.Empty;

        var ruleNumber = ruleId.Split('.').Last();
        var rulePrefix = ruleId.Replace("." + ruleNumber, string.Empty, StringComparison.OrdinalIgnoreCase);

        var descriptionWithoutUrl = urlStartIndex >= 0 ? description.Substring(0, urlStartIndex) : description;
        description = rulePrefix + ": " + descriptionWithoutUrl;

        return (
            Filename: fileAndPosition,
            Line: line,
            Column: column,
            RuleId: ruleNumber,
            Description: description,
            Url: url
        );
    }
}
