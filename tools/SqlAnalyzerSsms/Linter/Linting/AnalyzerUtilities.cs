using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft;
using Microsoft.VisualStudio.Threading;

namespace SqlAnalyzerSsms.Linter.Linting;

/// <summary>
/// Helper class for running analyzer on a string or file.
/// </summary>
internal class AnalyzerUtilities
{
    private static readonly Lazy<AnalyzerUtilities> _instance =
            new(() => new AnalyzerUtilities(), System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

    public static AnalyzerUtilities Instance => _instance.Value;

    private static string CreateTempFilePath() => Path.Combine(Path.GetTempPath(), $"tsqlanalyzerscratch-{Guid.NewGuid()}.sql");

    public async Task<List<SqlAnalyzerDiagnosticInfo>> AnalyzeAsync(string text, string rules, string sqlVersion, CancellationToken cancellationToken = default)
    {
        using var analyzer = new Process();
        var lineQueue = new AsyncQueue<string>();

        if (text?.Length > 8192)
        {
            // SQL analyzer has issues processing very large files.
            return [];
        }

        // Create a unique temp file for each analysis to support concurrent calls.
        var tempPath = CreateTempFilePath();

        try
        {
            try
            {
                await WriteTempFileAsync(tempPath, text, cancellationToken).ConfigureAwait(false);
            }
            catch (IOException ioex)
            {
                await ioex.LogAsync();

                // Failed to write temp file; cannot proceed with analysis.
                return [];
            }
            catch (UnauthorizedAccessException uex)
            {
                await uex.LogAsync();

                // No permission to write temp file; cannot proceed with analysis.
                return [];
            }

            StartAnalyzerProcess(analyzer, lineQueue, tempPath, rules, sqlVersion);

            List<SqlAnalyzerDiagnosticInfo> sqlDiagnostics;
            try
            {
                sqlDiagnostics = await ProcessAnalyzerQueueAsync(lineQueue, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                if (!analyzer.HasExited)
                {
                    try
                    {
                        analyzer.Kill();
                    }
                    catch (InvalidOperationException)
                    {
                        // Process has already exited or was not started; ignore.
                    }
                    catch (Win32Exception)
                    {
                        // Failed to kill the process; ignore to avoid throwing during cleanup.
                    }

                    try
                    {
                        await analyzer.WaitForExitAsync(cancellationToken);
                    }
                    catch (InvalidOperationException)
                    {
                        // Process has already exited; nothing more to do.
                    }
                }
            }

            return sqlDiagnostics;
        }
        finally
        {
            DeleteTempFile(tempPath);
        }
    }

    private static async Task WriteTempFileAsync(string path, string content, CancellationToken cancellationToken)
    {
        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true);
        var bytes = Encoding.UTF8.GetBytes(content);
        await stream.WriteAsync(bytes, 0, bytes.Length, cancellationToken).ConfigureAwait(false);
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
            // File may be locked; ignore cleanup failure.
        }
        catch (UnauthorizedAccessException)
        {
            // No permission to delete; ignore cleanup failure.
        }
    }

    private static void StartAnalyzerProcess(Process analyzer, AsyncQueue<string> lineQueue, string path, string rules, string sqlVersion)
    {
        var args = $"-n -i \"{path}\"";

        if (!string.IsNullOrWhiteSpace(rules))
        {
            args = args + $" -r Rules:{rules}";
        }

        if (!string.IsNullOrWhiteSpace(sqlVersion))
        {
            args = args + $" -s {sqlVersion}";
        }

        analyzer.StartInfo = new ProcessStartInfo()
        {
            FileName = "cmd.exe",
            Arguments = $"/c tsqlanalyze {args}",
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

    private static async Task<List<SqlAnalyzerDiagnosticInfo>> ProcessAnalyzerQueueAsync(AsyncQueue<string> lineQueue, CancellationToken cancellationToken)
    {
        Requires.NotNull(lineQueue, nameof(lineQueue));

        List<SqlAnalyzerDiagnosticInfo> diagnostics = new List<SqlAnalyzerDiagnosticInfo>();

        while (!(lineQueue.IsCompleted && lineQueue.IsEmpty))
        {
            string line;

            try
            {
                line = await lineQueue.DequeueAsync(cancellationToken);
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

        var parts = outputLine.Split([": "], 3, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 3)
        {
            return null;
        }

        // C:\Users\ErikEjlskovJensen(De\AppData\Local\Temp\tsqlanalyzerscratch.sql(23,1): SqlServer.Rules.SRN0007 : Index 'IFK_EmployeeReportsTo' does not follow the company naming standard. Please use a format that starts with IX_Employee*. (https://github.com/ErikEJ/SqlServer.Rules/blob/master/docs/Naming/SRN0007.md)
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

        var ruleIdParts = ruleId.Split('.');
        var ruleNumber = ruleIdParts[ruleIdParts.Length - 1];
        var rulePrefix = ruleId.Replace("." + ruleNumber, string.Empty);

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
