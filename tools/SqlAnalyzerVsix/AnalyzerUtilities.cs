using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ErikEJ.DacFX.TSQLAnalyzer.Protocol;
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
#pragma warning disable CA1001 // AnalyzerUtilities lifetime is extension-scoped singleton
internal sealed class AnalyzerUtilities
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly AsyncSemaphore requestLock = new(1);
    private Process? serverProcess;
    private StreamWriter? serverInput;
    private StreamReader? serverOutput;

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
        var serverDiagnostics = await AnalyzeWithServerModeAsync(fileUri.LocalPath, rules, sqlVersion, cancellationToken);
        return CreateDocumentDiagnosticsForClosedDocument(fileUri, serverDiagnostics);
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
        if (textDocument.Length > 8192)
        {
            // SQL analyzer has issues processing very large files.
            return Array.Empty<DocumentDiagnostic>();
        }

        var content = textDocument.Text.CopyToString();
        var tempPath = CreateTempFilePath();

        try
        {
            await File.WriteAllTextAsync(tempPath, content, Encoding.UTF8, cancellationToken);

            var serverDiagnostics = await AnalyzeWithServerModeAsync(tempPath, rules, sqlVersion, cancellationToken);
            return CreateDocumentDiagnosticsForOpenDocument(textDocument, serverDiagnostics);
        }
        finally
        {
            DeleteTempFile(tempPath);
        }
    }

    private static string CreateTempFilePath() => Path.Combine(Path.GetTempPath(), $"tsqlanalyzerscratch-{Path.GetRandomFileName()}.sql");

    private async Task<IEnumerable<SqlAnalyzerDiagnosticInfo>> AnalyzeWithServerModeAsync(string path, string? rules, string? sqlVersion, CancellationToken cancellationToken)
    {
        using var lockReleaser = await this.requestLock.EnterAsync(cancellationToken);

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!this.EnsureServerProcessStarted())
            {
                return [];
            }

            var request = new ServerRequest
            {
                Id = Guid.NewGuid().ToString("N"),
                Command = "analyze",
                Path = path,
                Rules = string.IsNullOrWhiteSpace(rules) ? null : $"Rules:{rules}",
                SqlVersion = string.IsNullOrWhiteSpace(sqlVersion) ? null : sqlVersion,
            };

            var requestJson = JsonSerializer.Serialize(request);

            if (this.serverInput is null)
            {
                throw new InvalidOperationException("Analyzer server input stream is not available.");
            }

            await this.serverInput.WriteLineAsync(requestJson);
            await this.serverInput.FlushAsync(cancellationToken);

            var response = await this.ReadResponseAsync(request.Id, cancellationToken);
            if (response is null)
            {
                return [];
            }

            if (!string.Equals(response.Status, "success", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(response.Error))
                {
                    throw new InvalidOperationException(response.Error);
                }

                return [];
            }

            return ConvertProblemsToDiagnostics(response.Problems);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (ObjectDisposedException)
        {
            this.ResetServerProcess();
            return [];
        }
        catch (InvalidOperationException)
        {
            this.ResetServerProcess();
            throw;
        }
        catch (IOException)
        {
            this.ResetServerProcess();
            return [];
        }
    }

    private static IEnumerable<SqlAnalyzerDiagnosticInfo> ConvertProblemsToDiagnostics(IList<ServerProblem>? problems)
    {
        if (problems is null || problems.Count == 0)
        {
            return [];
        }

        var diagnostics = new List<SqlAnalyzerDiagnosticInfo>(problems.Count);

        foreach (var problem in problems)
        {
            if (problem is null)
            {
                continue;
            }

            var startLine = Math.Max(problem.Line, 1) - 1;
            var startColumn = Math.Max(problem.Column, 1) - 1;
            var endLine = Math.Max(problem.EndLine, problem.Line) - 1;
            var endColumn = Math.Max(problem.EndColumn, problem.Column) - 1;
            var range = new Microsoft.VisualStudio.RpcContracts.Utilities.Range(startLine, startColumn, endLine, endColumn);

            var fullRule = string.IsNullOrWhiteSpace(problem.Rule) ? "unknown" : problem.Rule;
            var errorCode = fullRule;
            var separatorIndex = fullRule.LastIndexOf('.');
            var rulePrefix = string.Empty;

            if (separatorIndex > 0 && separatorIndex < fullRule.Length - 1)
            {
                rulePrefix = fullRule.Substring(0, separatorIndex);
                errorCode = fullRule.Substring(separatorIndex + 1);
            }

            var message = string.IsNullOrWhiteSpace(problem.Message)
                ? fullRule
                : string.IsNullOrWhiteSpace(rulePrefix) ? problem.Message : rulePrefix + ": " + problem.Message;

            Uri? helpLink = null;
            if (!string.IsNullOrWhiteSpace(problem.HelpLink)
                && Uri.TryCreate(problem.HelpLink, UriKind.Absolute, out var parsedHelpLink))
            {
                helpLink = parsedHelpLink;
            }

            diagnostics.Add(new SqlAnalyzerDiagnosticInfo(range, message, errorCode, helpLink));
        }

        return diagnostics;
    }

    private bool EnsureServerProcessStarted()
    {
        if (this.serverProcess is not null && !this.serverProcess.HasExited)
        {
            return true;
        }

        this.ResetServerProcess();

        var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = "/c dnx ErikEJ.DacFX.TSQLAnalyzer.Cli --yes -- --server-mode",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        try
        {
            if (!process.Start())
            {
                process.Dispose();
                return false;
            }
        }
        catch (Win32Exception ex)
        {
            process.Dispose();
            throw new InvalidOperationException(message: ex.Message, innerException: ex);
        }

        this.serverProcess = process;
        this.serverInput = process.StandardInput;
        this.serverOutput = process.StandardOutput;
        _ = DrainServerErrorAsync(process.StandardError);

        return true;
    }

    private async Task<ServerResponse?> ReadResponseAsync(string requestId, CancellationToken cancellationToken)
    {
        while (true)
        {
            var line = await this.serverOutput!.ReadLineAsync(cancellationToken);
            if (line is null)
            {
                this.ResetServerProcess();
                return null;
            }

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            ServerResponse? response;
            try
            {
                response = JsonSerializer.Deserialize<ServerResponse>(line, JsonSerializerOptions);
            }
            catch (JsonException)
            {
                continue;
            }

            if (response is null)
            {
                continue;
            }

            if (string.Equals(response.Id, requestId, StringComparison.Ordinal))
            {
                return response;
            }

            if (string.Equals(response.Status, "error", StringComparison.OrdinalIgnoreCase)
                && (string.IsNullOrEmpty(response.Id) || string.Equals(response.Id, "unknown", StringComparison.OrdinalIgnoreCase)))
            {
                this.ResetServerProcess();
                return response;
            }
        }
    }

    private static async Task DrainServerErrorAsync(StreamReader errorReader)
    {
        try
        {
            while (true)
            {
                var line = await errorReader.ReadLineAsync(CancellationToken.None);
                if (line is null)
                {
                    break;
                }
            }
        }
        catch (ObjectDisposedException)
        {
            // Process is shutting down.
        }
        catch (IOException)
        {
            // Process ended or stream closed.
        }
    }

    private void ResetServerProcess()
    {
        var process = this.serverProcess;
        this.serverProcess = null;

        try
        {
            this.serverInput?.Dispose();
        }
        catch (ObjectDisposedException)
        {
        }

        try
        {
            this.serverOutput?.Dispose();
        }
        catch (ObjectDisposedException)
        {
        }

        this.serverInput = null;
        this.serverOutput = null;

        if (process is null)
        {
            return;
        }

        try
        {
            if (!process.HasExited)
            {
                process.Kill();
                process.WaitForExit(1000);
            }
        }
        catch (InvalidOperationException)
        {
        }
        catch (Win32Exception)
        {
        }
        finally
        {
            process.Dispose();
        }
    }

    private static void DeleteTempFile(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
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
                ProviderName = Strings.AnalyzerWindowName,
                HelpLink = diagnostic.HelpLink?.ToString(),
            };
        }
    }
    #pragma warning restore CA1001

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

        // Create the range with start and optional end positions
        // If end positions are provided, use them; otherwise, range ends at the start position
        var range = parsed.Value.EndLine.HasValue && parsed.Value.EndColumn.HasValue
            ? new Microsoft.VisualStudio.RpcContracts.Utilities.Range(
                startLine: parsed.Value.Line - 1,
                startColumn: parsed.Value.Column - 1,
                endLine: parsed.Value.EndLine.Value - 1,
                endColumn: parsed.Value.EndColumn.Value - 1)
            : new Microsoft.VisualStudio.RpcContracts.Utilities.Range(
                startLine: parsed.Value.Line - 1,
                startColumn: parsed.Value.Column - 1);

        return new SqlAnalyzerDiagnosticInfo(
            range: range,
            message: parsed.Value.Description,
            errorCode: parsed.Value.RuleId,
            helpLink: !string.IsNullOrEmpty(parsed.Value.Url) ? new Uri(parsed.Value.Url) : null);
    }

    /// <summary>
    /// Parses a SQL analyzer output line and extracts its components.
    /// Supports both output formats:
    /// - Old format: file(line,column): RuleId : Description
    /// - New format: file(line,column,endLine,endColumn): RuleId : Description
    /// </summary>
    /// <param name="outputLine">The output line to parse</param>
    /// <returns>A tuple containing the parsed components, or null if parsing fails</returns>
    public static (string Filename, int Line, int Column, int? EndLine, int? EndColumn, string RuleId, string Description, string Url)? ParseSqlAnalyzerOutput(string outputLine)
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

        // Example formats:
        // Old: C:\Users\ErikEjlskovJensen(De\AppData\Local\Temp\tsqlanalyzerscratch.sql(23,1): SqlServer.Rules.SRN0007 : Index 'IFK_EmployeeReportsTo' does not follow the company naming standard. Please use a format that starts with IX_Employee*. (https://github.com/ErikEJ/SqlServer.Rules/blob/master/docs/Naming/SRN0007.md)
        // New: C:\Users\ErikEjlskovJensen(De\AppData\Local\Temp\tsqlanalyzerscratch.sql(23,1,23,7): SqlServer.Rules.SRN0007 : Index 'IFK_EmployeeReportsTo' does not follow the company naming standard. Please use a format that starts with IX_Employee*. (https://github.com/ErikEJ/SqlServer.Rules/blob/master/docs/Naming/SRN0007.md)
        var fileAndPosition = parts[0].Trim();
        var lineColumnStart = fileAndPosition.LastIndexOf('(');
        var lineColumnEnd = fileAndPosition.LastIndexOf(')');
        if (lineColumnStart < 0 || lineColumnEnd < 0 || lineColumnEnd <= lineColumnStart)
        {
            return null;
        }

        var lineColumn = fileAndPosition.Substring(lineColumnStart + 1, lineColumnEnd - lineColumnStart - 1);
        var lineColumnParts = lineColumn.Split(',');

        int line, column;
        int? endLine = null;
        int? endColumn = null;

        // Parse based on the number of comma-separated values
        if (lineColumnParts.Length == 2)
        {
            // Old format: (line,column)
            if (!int.TryParse(lineColumnParts[0], out line) ||
                !int.TryParse(lineColumnParts[1], out column))
            {
                return null;
            }
        }
        else if (lineColumnParts.Length == 4)
        {
            // New format: (line,column,endLine,endColumn)
            if (!int.TryParse(lineColumnParts[0], out line) ||
                !int.TryParse(lineColumnParts[1], out column) ||
                !int.TryParse(lineColumnParts[2], out int parsedEndLine) ||
                !int.TryParse(lineColumnParts[3], out int parsedEndColumn))
            {
                return null;
            }

            endLine = parsedEndLine;
            endColumn = parsedEndColumn;
        }
        else
        {
            // Unknown format
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
            EndLine: endLine,
            EndColumn: endColumn,
            RuleId: ruleNumber,
            Description: description,
            Url: url
        );
    }
}
