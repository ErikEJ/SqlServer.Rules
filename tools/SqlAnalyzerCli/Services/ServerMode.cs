using System.Text.Json;
using ErikEJ.DacFX.TSQLAnalyzer;
using ErikEJ.DacFX.TSQLAnalyzer.Extensions;
using ErikEJ.DacFX.TSQLAnalyzer.Protocol;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;

#pragma warning disable CA1031 // Do not catch general exception types - server mode needs to remain stable

namespace SqlAnalyzerCli.Services;

/// <summary>
/// Implements server mode for the CLI, accepting analysis requests via stdin
/// and returning results via stdout using newline-delimited JSON protocol.
/// </summary>
internal static class ServerMode
{
    private const string RulesPrefix = "Rules:";
    private const string ErrorRulePrefix = "+!";

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
        var response = BuildAnalyzeResponse(request);
        await WriteResponseAsync(response);
    }

    /// <summary>
    /// Runs analysis for a single request and returns the response without touching the console
    /// transport, so the logic can be exercised in isolation.
    /// When <see cref="ServerRequest.Content"/> is set it is analyzed in-memory and takes
    /// precedence over <see cref="ServerRequest.Path"/>.
    /// </summary>
    /// <param name="request">The analyze request.</param>
    /// <returns>A success or error response.</returns>
    internal static ServerResponse BuildAnalyzeResponse(ServerRequest request)
    {
        var hasContent = request.Content != null;

        if (!hasContent && string.IsNullOrWhiteSpace(request.Path))
        {
            return ErrorResponse(request.Id, "Path or content is required for analyze command");
        }

        // An empty (non-null) content buffer is a valid in-memory request: no SQL → no problems.
        if (hasContent && string.IsNullOrWhiteSpace(request.Content))
        {
            return new ServerResponse { Id = request.Id, Status = "success", Problems = [] };
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
                    return ErrorResponse(request.Id, $"Invalid SQL version: {request.SqlVersion}");
                }
            }

            // Set up analyzer options
            var normalizedRules = NormalizeRules(request.Rules);
            var analyzerOptions = new AnalyzerOptions
            {
                Rules = normalizedRules,
                SqlVersion = sqlVersion,
                AdditionalAnalyzers = request.AdditionalAnalyzers?.ToList(),
            };

            if (hasContent)
            {
                // Analyze the in-memory buffer content directly (no temp file).
                analyzerOptions.Scripts = null;
                analyzerOptions.Script = request.Content;
            }
            else if (File.Exists(request.Path) || Directory.Exists(request.Path))
            {
                analyzerOptions.Scripts = [request.Path!];
            }
            else
            {
                return ErrorResponse(request.Id, $"Path not found: {request.Path}");
            }

            // Run analysis
            var analyzerFactory = new AnalyzerFactory(analyzerOptions);
            var result = analyzerFactory.Analyze();

            if (result?.Result == null)
            {
                return ErrorResponse(request.Id, "Analysis returned null result");
            }

            // Check for errors
            var errors = result.Result.GetAllErrors();
            if (errors.Any())
            {
                var errorMessages = string.Join("; ", errors.Select(e => e.GetOutputMessage()));
                return ErrorResponse(request.Id, $"Analysis errors: {errorMessages}");
            }

            // Build problems list
            var problems = new List<ServerProblem>();
            var errorRules = ParseErrorRules(normalizedRules);

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
                        Severity = GetEffectiveSeverity(problem, errorRules),
                        HelpLink = helpLink,
                        File = problem.SourceName,
                    });
                }
            }

            // Build success response
            return new ServerResponse
            {
                Id = request.Id,
                Status = "success",
                Problems = problems,
            };
        }
        catch (ArgumentException ex)
        {
            return ErrorResponse(request.Id, ex.Message);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Analysis error: {ex}");
            return ErrorResponse(request.Id, $"Analysis failed: {ex.Message}");
        }
    }

    private static ServerResponse ErrorResponse(string requestId, string errorMessage)
        => new()
        {
            Id = requestId,
            Status = "error",
            Error = errorMessage,
        };

    /// <summary>
    /// Normalizes a server-mode rules expression so callers (e.g. a language client) do not have to
    /// include the leading <c>Rules:</c> prefix that the analyzer expects. When the expression is
    /// empty it is passed through unchanged; when it already starts with <c>Rules:</c> it is kept as
    /// is; otherwise the prefix is added.
    /// </summary>
    /// <param name="rules">The rules expression supplied by the client, e.g. "-SqlServer.Rules.SRD0004".</param>
    /// <returns>A rules expression prefixed with <c>Rules:</c>, or an empty string.</returns>
    internal static string NormalizeRules(string? rules)
    {
        if (string.IsNullOrWhiteSpace(rules))
        {
            return string.Empty;
        }

        var trimmed = rules.Trim();

        return trimmed.StartsWith(RulesPrefix, StringComparison.OrdinalIgnoreCase)
            ? trimmed
            : RulesPrefix + trimmed;
    }

    /// <summary>
    /// Parses a normalized rules expression for the rule ids promoted to error level using the
    /// <c>+!</c> prefix (e.g. "Rules:+!SqlServer.Rules.SRD0004"). Wildcard entries ending in <c>*</c>
    /// are retained so they can be matched by prefix.
    /// </summary>
    /// <param name="normalizedRules">A rules expression already prefixed with <c>Rules:</c>.</param>
    /// <returns>The set of rule ids (and wildcard prefixes) promoted to error level.</returns>
    internal static HashSet<string> ParseErrorRules(string? normalizedRules)
    {
        var errorRules = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrEmpty(normalizedRules) || normalizedRules.Length <= RulesPrefix.Length)
        {
            return errorRules;
        }

        var rulesExpression = normalizedRules.Substring(RulesPrefix.Length);

        foreach (var rule in rulesExpression
            .Split([';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(rule => rule.StartsWith(ErrorRulePrefix, StringComparison.OrdinalIgnoreCase) && rule.Length > ErrorRulePrefix.Length))
        {
            errorRules.Add(rule[ErrorRulePrefix.Length..]);
        }

        return errorRules;
    }

    /// <summary>
    /// Determines the effective severity for a problem, promoting it to <c>Error</c> when its rule id
    /// matches one of the <c>+!</c> entries (directly or by wildcard prefix).
    /// </summary>
    /// <param name="problem">The analyzed problem.</param>
    /// <param name="errorRules">The set of rule ids/wildcards promoted to error level.</param>
    /// <returns>The severity string ("Error" or the problem's inherent severity).</returns>
    internal static string GetEffectiveSeverity(SqlRuleProblem problem, HashSet<string> errorRules)
    {
        if (errorRules.Count > 0)
        {
            if (errorRules.Contains(problem.RuleId))
            {
                return SqlRuleProblemSeverity.Error.ToString();
            }

            if (errorRules
                .Where(r => r.EndsWith('*'))
                .Any(r => problem.RuleId.StartsWith(r[..^1], StringComparison.OrdinalIgnoreCase)))
            {
                return SqlRuleProblemSeverity.Error.ToString();
            }
        }

        return problem.Severity.ToString();
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
