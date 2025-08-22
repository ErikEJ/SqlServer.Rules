namespace SqlAnalyzer;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
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
    private static readonly Regex LinterOutputRegex = new(@"(?<File>[^:]+):(?<Line>\d*)(:(?<Column>\d*))? (?<Error>.*)/(?<Description>.*)", RegexOptions.Compiled);

    // Regex to parse T-SQL analyzer CLI output format:
    // filename(line,column): ruleid : description
    private static readonly Regex SqlAnalyzerOutputRegex = new(
        @"^(?<filename>.+?)\((?<line>\d+),(?<column>\d+)\):\s*(?<ruleid>[^:]+)\s*:\s*(?<description>.*)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);



    /// <summary>
    /// Runs markdown linter on a file uri and returns diagnostic entries.
    /// </summary>
    /// <param name="fileUri">File uri to run markdown linter on.</param>
    /// <param name="cancellationToken">Cancellation token to monitor.</param>
    /// <returns>an enumeration of <see cref="DocumentDiagnostic"/> entries for warnings in the markdown file.</returns>
    public async Task<IEnumerable<DocumentDiagnostic>> RunAnalyzerOnFileAsync(Uri fileUri, CancellationToken cancellationToken)
    {
        using var linter = new Process();
        var lineQueue = new AsyncQueue<string>();

        string args = "/c \"tsqlanalyze -n -i" +
            $" \"{fileUri.LocalPath}\"\"";

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

        var markdownDiagnostics = await ProcessLinterQueueAsync(lineQueue);
        return CreateDocumentDiagnosticsForClosedDocument(fileUri, markdownDiagnostics);
    }

    /// <summary>
    /// Runs markdown linter on a given text document and returns diagnostic entries.
    /// </summary>
    /// <param name="textDocument">Document to run markdown linter on.</param>
    /// <param name="cancellationToken">Cancellation token to monitor.</param>
    /// <returns>an enumeration of <see cref="DocumentDiagnostic"/> entries for warnings in the markdown file.</returns>
    public async Task<IEnumerable<DocumentDiagnostic>> RunLinterOnDocumentAsync(ITextDocumentSnapshot textDocument, CancellationToken cancellationToken)
    {
        using var linter = new Process();
        var lineQueue = new AsyncQueue<string>();

        var content = textDocument.Text.CopyToString();

        ////var snapshot = await this.settingsObserver.GetSnapshotAsync(cancellationToken);
        ////string disabledRules = snapshot.DisabledRules.ValueOrDefault(string.Empty);

        ////string args = "/k \"npx markdownlint-cli --stdin" +
        ////    (disabledRules.Length > 0 ? $" --disable {disabledRules}" : string.Empty) + "\"";

        ////linter.StartInfo = new ProcessStartInfo()
        ////{
        ////    FileName = "cmd.exe",
        ////    Arguments = args,
        ////    RedirectStandardError = true,
        ////    RedirectStandardInput = true,
        ////    UseShellExecute = false,
        ////    CreateNoWindow = true,
        ////};

        ////linter.EnableRaisingEvents = true;
        ////linter.ErrorDataReceived += new DataReceivedEventHandler((sender, e) =>
        ////{
        ////    if (e.Data is not null)
        ////    {
        ////        lineQueue.Enqueue(e.Data);
        ////    }
        ////    else
        ////    {
        ////        lineQueue.Complete();
        ////    }
        ////});

        ////try
        ////{
        ////    linter.Start();
        ////    linter.BeginErrorReadLine();
        ////    linter.StandardInput.AutoFlush = true;
        ////    await linter.StandardInput.WriteAsync(content);

        ////    linter.StandardInput.Close();
        ////}
        ////catch (Win32Exception ex)
        ////{
        ////    throw new InvalidOperationException(message: ex.Message, innerException: ex);
        ////}

        var markdownDiagnostics = await ProcessLinterQueueAsync(lineQueue);
        return CreateDocumentDiagnosticsForOpenDocument(textDocument, markdownDiagnostics);
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
                ProviderName = "T-SQL Analyzer",
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
                ProviderName = "T-SQL Analyzer",
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

        // TODO - Massage the description and ruleid to match the expected format.
        return new SqlAnalyzerDiagnosticInfo(
            range: new Microsoft.VisualStudio.RpcContracts.Utilities.Range(startLine: parsed.Value.Line, startColumn: parsed.Value.Column),
            message: parsed.Value.Description,
            errorCode: parsed.Value.RuleId);
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
