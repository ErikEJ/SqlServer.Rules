using System.Text.Json;
using ErikEJ.DacFX.TSQLAnalyzer;
using ErikEJ.DacFX.TSQLAnalyzer.Extensions;
using ErikEJ.DacFX.TSQLAnalyzer.Protocol;
using Microsoft.SqlServer.Dac.Model;

#pragma warning disable CA1031 // Do not catch general exception types - server mode needs to remain stable

namespace SqlAnalyzerCli.Services;

/// <summary>
/// Implements server mode for the CLI, accepting analysis requests via stdin
/// and returning results via stdout using newline-delimited JSON protocol.
/// </summary>
internal static class ServerMode
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    /// <summary>
    /// Run the server mode loop, reading requests from stdin and writing responses to stdout.
    /// Continues until receiving a "shutdown" command or stdin is closed.
    /// </summary>
    /// <returns>Exit code: 0 for success, 1 for fatal error.</returns>
    public static async Task<int> RunAsync()
    {
        // Write ready signal to stderr (stdout is reserved for protocol)
        await Console.Error.WriteLineAsync("TSQLAnalyzer server mode started. Ready for requests.");
        await Console.Error.FlushAsync();

        try
        {
            while (true)
            {
                // Read one line from stdin (blocking)
                var line = await Console.In.ReadLineAsync();

                // EOF or null means stdin closed - exit gracefully
                if (line is null)
                {
                    await Console.Error.WriteLineAsync("stdin closed. Shutting down.");
                    break;
                }

                // Skip empty lines
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                try
                {
                    // Parse request
                    var request = JsonSerializer.Deserialize<ServerRequest>(line, JsonOptions);

                    if (request is null)
                    {
                        await WriteErrorAsync("unknown", "Failed to parse request JSON");
                        continue;
                    }

                    // Handle shutdown command
                    if (string.Equals(request.Command, "shutdown", StringComparison.OrdinalIgnoreCase))
                    {
                        var shutdownResponse = new ServerResponse
                        {
                            Id = request.Id,
                            Status = "shutdown",
                        };
                        await WriteResponseAsync(shutdownResponse);
                        break;
                    }

                    // Handle analyze command
                    if (string.Equals(request.Command, "analyze", StringComparison.OrdinalIgnoreCase))
                    {
                        await HandleAnalyzeAsync(request);
                    }
                    else
                    {
                        await WriteErrorAsync(request.Id, $"Unknown command: {request.Command}");
                    }
                }
                catch (JsonException ex)
                {
                    await WriteErrorAsync("unknown", $"JSON parse error: {ex.Message}");
                }
                catch (Exception ex)
                {
                    await Console.Error.WriteLineAsync($"Error processing request: {ex}");
                    await WriteErrorAsync("unknown", $"Internal error: {ex.Message}");
                }
            }

            return 0;
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Fatal error in server mode: {ex}");
            return 1;
        }
    }

    private static async Task HandleAnalyzeAsync(ServerRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Path))
        {
            await WriteErrorAsync(request.Id, "Path is required for analyze command");
            return;
        }

        try
        {
            // Parse SQL version
            var sqlVersion = SqlServerVersion.Sql160; // Default
            if (!string.IsNullOrWhiteSpace(request.SqlVersion))
            {
                if (Enum.TryParse<SqlServerVersion>(request.SqlVersion, ignoreCase: true, out var parsedVersion))
                {
                    sqlVersion = parsedVersion;
                }
                else
                {
                    await WriteErrorAsync(request.Id, $"Invalid SQL version: {request.SqlVersion}");
                    return;
                }
            }

            // Set up analyzer options
            var analyzerOptions = new AnalyzerOptions
            {
                Rules = request.Rules ?? string.Empty,
                SqlVersion = sqlVersion,
                AdditionalAnalyzers = request.AdditionalAnalyzers?.ToList(),
            };

            // Determine if path is a file or directory
            if (File.Exists(request.Path))
            {
                analyzerOptions.Scripts = [request.Path];
            }
            else if (Directory.Exists(request.Path))
            {
                analyzerOptions.Scripts = [request.Path];
            }
            else
            {
                await WriteErrorAsync(request.Id, $"Path not found: {request.Path}");
                return;
            }

            // Run analysis
            var analyzerFactory = new AnalyzerFactory(analyzerOptions);
            var result = analyzerFactory.Analyze();

            if (result?.Result == null)
            {
                await WriteErrorAsync(request.Id, "Analysis returned null result");
                return;
            }

            // Check for errors
            var errors = result.Result.GetAllErrors();
            if (errors.Any())
            {
                var errorMessages = string.Join("; ", errors.Select(e => e.GetOutputMessage()));
                await WriteErrorAsync(request.Id, $"Analysis errors: {errorMessages}");
                return;
            }

            // Build problems list
            var problems = new List<ServerProblem>();

            if (result.Result.AnalysisSucceeded)
            {
                foreach (var problem in result.Result.Problems)
                {
                    var (endLine, endColumn) = problem.GetEndPosition(result);

                    var (message, helpLink) = ExtractMessageAndHelpLink(problem.Description);

                    problems.Add(new ServerProblem
                    {
                        Rule = problem.RuleId ?? "unknown",
                        Line = problem.StartLine,
                        Column = result.GetAdjustedColumn(problem.StartLine, problem.StartColumn, problem.SourceName),
                        EndLine = endLine,
                        EndColumn = endColumn,
                        Message = message,
                        HelpLink = helpLink,
                        File = problem.SourceName,
                    });
                }
            }

            // Write success response
            var response = new ServerResponse
            {
                Id = request.Id,
                Status = "success",
                Problems = problems,
            };

            await WriteResponseAsync(response);
        }
        catch (ArgumentException ex)
        {
            await WriteErrorAsync(request.Id, ex.Message);
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Analysis error: {ex}");
            await WriteErrorAsync(request.Id, $"Analysis failed: {ex.Message}");
        }
    }

    private static (string Message, string? HelpLink) ExtractMessageAndHelpLink(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return (string.Empty, null);
        }

        var trimmed = description.Trim();
        var linkStartIndex = trimmed.LastIndexOf(" (http", StringComparison.OrdinalIgnoreCase);

        if (linkStartIndex < 0 || !trimmed.EndsWith(")", StringComparison.Ordinal))
        {
            return (trimmed, null);
        }

        var candidateLink = trimmed.Substring(linkStartIndex + 2, trimmed.Length - linkStartIndex - 3);
        if (!Uri.TryCreate(candidateLink, UriKind.Absolute, out _))
        {
            return (trimmed, null);
        }

        var message = trimmed.Substring(0, linkStartIndex).TrimEnd();
        return (message, candidateLink);
    }

    private static async Task WriteResponseAsync(ServerResponse response)
    {
        var json = JsonSerializer.Serialize(response, JsonOptions);
        await Console.Out.WriteLineAsync(json);
        await Console.Out.FlushAsync();
    }

    private static async Task WriteErrorAsync(string requestId, string errorMessage)
    {
        var response = new ServerResponse
        {
            Id = requestId,
            Status = "error",
            Error = errorMessage,
        };
        await WriteResponseAsync(response);
    }
}
