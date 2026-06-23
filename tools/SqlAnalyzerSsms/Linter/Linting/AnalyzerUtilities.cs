using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ErikEJ.DacFX.TSQLAnalyzer.Protocol;
using Microsoft.VisualStudio.Threading;

namespace SqlAnalyzerSsms.Linter.Linting;

#pragma warning disable SA1309 // Field names should not begin with underscore - we prefer this for private fields
#pragma warning disable IDE1006 // Naming rule violation
internal sealed class AnalyzerUtilities
{
    private static readonly Lazy<AnalyzerUtilities> _instance =
            new(() => new AnalyzerUtilities(), System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly SemaphoreSlim _requestLock = new(1, 1);

    private Process? _serverProcess;
    private StreamWriter? _serverInput;
    private StreamReader? _serverOutput;

    public static AnalyzerUtilities Instance => _instance.Value;

    private static string CreateTempFilePath() => Path.Combine(Path.GetTempPath(), $"tsqlanalyzerscratch-{Guid.NewGuid()}.sql");

    public async Task<List<SqlAnalyzerDiagnosticInfo>> AnalyzeAsync(string text, string rules, string sqlVersion, CancellationToken cancellationToken = default)
    {
        if (text == null)
        {
            return [];
        }

        if (text.Length > 8192)
        {
            // SQL analyzer has issues processing very large files.
            return [];
        }

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
                return [];
            }
            catch (UnauthorizedAccessException uex)
            {
                await uex.LogAsync();
                return [];
            }

            return await AnalyzeWithServerModeAsync(tempPath, rules, sqlVersion, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            DeleteTempFile(tempPath);
        }
    }

    private async Task<List<SqlAnalyzerDiagnosticInfo>> AnalyzeWithServerModeAsync(string path, string rules, string sqlVersion, CancellationToken cancellationToken)
    {
        await _requestLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!EnsureServerProcessStarted())
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

            if (_serverInput is null)
            {
                throw new InvalidOperationException("Analyzer server input stream is not available.");
            }

            await _serverInput.WriteLineAsync(requestJson).ConfigureAwait(false);
            await _serverInput.FlushAsync().ConfigureAwait(false);

            var response = await ReadResponseAsync(request.Id, cancellationToken).ConfigureAwait(false);
            if (response is null)
            {
                return [];
            }

            if (!string.Equals(response.Status, "success", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(response.Error))
                {
                    await new InvalidOperationException($"Server mode analysis failed: {response.Error}").LogAsync().ConfigureAwait(false);
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
            ResetServerProcess();
            return [];
        }
        catch (InvalidOperationException ex)
        {
            await ex.LogAsync().ConfigureAwait(false);
            ResetServerProcess();
            return [];
        }
        catch (IOException ex)
        {
            await ex.LogAsync().ConfigureAwait(false);
            ResetServerProcess();
            return [];
        }
        finally
        {
            _requestLock.Release();
        }
    }

    private static List<SqlAnalyzerDiagnosticInfo> ConvertProblemsToDiagnostics(IList<ServerProblem>? problems)
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
            var range = new Microsoft.VisualStudio.RpcContracts.Utilities.Range(
                startLine: startLine,
                startColumn: startColumn,
                endLine: endLine,
                endColumn: endColumn);

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
        if (_serverProcess is not null && !_serverProcess.HasExited)
        {
            return true;
        }

        ResetServerProcess();

        var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = "tool exec ErikEJ.DacFX.TSQLAnalyzer.Cli --yes -- --server-mode",
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

        _serverProcess = process;
        _serverInput = process.StandardInput;
        _serverOutput = process.StandardOutput;
        _ = DrainServerErrorAsync(process.StandardError);

        return true;
    }

    private async Task<ServerResponse?> ReadResponseAsync(string requestId, CancellationToken cancellationToken)
    {
        while (true)
        {
            var line = await _serverOutput!.ReadLineAsync().WithCancellation(cancellationToken).ConfigureAwait(false);
            if (line is null)
            {
                ResetServerProcess();
                return null;
            }

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            ServerResponse? response;
            try
            {
                response = JsonSerializer.Deserialize<ServerResponse>(line, _jsonSerializerOptions);
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
                ResetServerProcess();
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
                var line = await errorReader.ReadLineAsync().ConfigureAwait(false);
                if (line is null)
                {
                    break;
                }

                if (!string.IsNullOrWhiteSpace(line))
                {
                    await new InvalidOperationException($"Analyzer server stderr: {line}").LogAsync().ConfigureAwait(false);
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
        var process = _serverProcess;
        _serverProcess = null;

        try
        {
            _serverInput?.Dispose();
        }
        catch (ObjectDisposedException)
        {
        }

        try
        {
            _serverOutput?.Dispose();
        }
        catch (ObjectDisposedException)
        {
        }

        _serverInput = null;
        _serverOutput = null;

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
}
#pragma warning restore SA1309 // Field names should not begin with underscore
#pragma warning restore IDE1006 // Naming rule violation
