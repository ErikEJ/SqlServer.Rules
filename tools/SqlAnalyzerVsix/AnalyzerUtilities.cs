namespace SqlAnalyzer;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft;
using Microsoft.VisualStudio.Extensibility.Editor;
using Microsoft.VisualStudio.Extensibility.Languages;
using Microsoft.VisualStudio.RpcContracts.DiagnosticManagement;
using Microsoft.VisualStudio.Threading;

#pragma warning disable VSEXTPREVIEW_SETTINGS // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

/// <summary>
/// Helper class for running linter on a string or file.
/// </summary>
internal class AnalyzerUtilities
{
    // Regex to parse T-SQL analyzer CLI output format:
    // filename(line,column): ruleid : description
    private static readonly Regex SqlAnalyzerOutputRegex = new(
        @"^(?<filename>.+?)\((?<line>\d+),(?<column>\d+)\):\s*(?<ruleid>[^:]+)\s*:\s*(?<description>.*)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

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
        using var linter = new Process();
        var lineQueue = new AsyncQueue<string>();

        StartLinterProcess(linter, lineQueue, fileUri.LocalPath, rules, sqlVersion);

        var markdownDiagnostics = await ProcessLinterQueueAsync(lineQueue);
        return CreateDocumentDiagnosticsForClosedDocument(fileUri, markdownDiagnostics);
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
        using var linter = new Process();
        var lineQueue = new AsyncQueue<string>();

        if (textDocument.Length > 8192)
        {
            // SQL analyzer has issues processing very large files.
            return Array.Empty<DocumentDiagnostic>();
        }

        var content = textDocument.Text.CopyToString();

        await File.WriteAllTextAsync(tempPath, content, Encoding.UTF8, cancellationToken);

        StartLinterProcess(linter, lineQueue, tempPath, rules, sqlVersion);

        var markdownDiagnostics = await ProcessLinterQueueAsync(lineQueue);
        return CreateDocumentDiagnosticsForOpenDocument(textDocument, markdownDiagnostics);
    }

    private static void StartLinterProcess(Process linter, AsyncQueue<string> lineQueue, string path, string? rules, string? sqlVersion)
    {
        string args = "/c \"tsqlanalyze -n -i" +
            $" \"{path}\"\"";

        if (!string.IsNullOrWhiteSpace(rules))
        {
            args = args + $" -r Rules:{rules}";
        }

        if (!string.IsNullOrWhiteSpace(sqlVersion))
        {
            args = args + $" -s {sqlVersion}";
        }

        linter.StartInfo = new ProcessStartInfo()
        {
            FileName = "cmd.exe",
            Arguments = args,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        linter.EnableRaisingEvents = true;
        linter.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
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
            linter.Start();
            linter.BeginOutputReadLine();
        }
        catch (Win32Exception ex)
        {
            throw new InvalidOperationException(message: ex.Message, innerException: ex);
        }
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
                ProviderName = Strings.MarkdownLinterWindowName,
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
                ProviderName = Strings.MarkdownLinterWindowName,
            };
        }
    }

    private static async Task<IEnumerable<SqlAnalyzerDiagnosticInfo>> ProcessLinterQueueAsync(AsyncQueue<string> lineQueue)
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

            var diagnostic = line is not null ? GetDiagnosticFromLinterOutput(line) : null;
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

    private static SqlAnalyzerDiagnosticInfo? GetDiagnosticFromLinterOutput(string outputLine)
    {
        Requires.NotNull(outputLine, nameof(outputLine));

        var parsed = ParseSqlAnalyzerOutput(outputLine);

        if (parsed is null)
        {
            // The output line does not match the expected format.
            return null;
        }

        var ruleId = parsed.Value.RuleId.Split('.').Last();
        var rulePrefix = parsed.Value.RuleId.Replace("." + ruleId, string.Empty, StringComparison.OrdinalIgnoreCase);
        var description = rulePrefix + ": " + parsed.Value.Description;

        return new SqlAnalyzerDiagnosticInfo(
            range: new Microsoft.VisualStudio.RpcContracts.Utilities.Range(startLine: parsed.Value.Line - 1, startColumn: parsed.Value.Column - 1),
            message: description,
            errorCode: ruleId);
    }

    /// <summary>
    /// Parses a SQL analyzer output line and extracts its components.
    /// </summary>
    /// <param name="outputLine">The output line to parse</param>
    /// <returns>A tuple containing the parsed components, or null if parsing fails</returns>
    public static (string Filename, int Line, int Column, string RuleId, string Description)? ParseSqlAnalyzerOutput(string outputLine)
    {
        if (string.IsNullOrWhiteSpace(outputLine))
        {
            return null;
        }

        var match = SqlAnalyzerOutputRegex.Match(outputLine);
        if (!match.Success)
        {
            return null;
        }

        if (!int.TryParse(match.Groups["line"].Value, out int line) ||
            !int.TryParse(match.Groups["column"].Value, out int column))
        {
            return null;
        }

        return (
            Filename: match.Groups["filename"].Value,
            Line: line,
            Column: column,
            RuleId: match.Groups["ruleid"].Value.Trim(),
            Description: match.Groups["description"].Value.Trim()
        );
    }
}
