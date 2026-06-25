using System.Text.Json.Serialization;

#pragma warning disable SA1649 // File name should match first type name - grouped protocol DTOs
#pragma warning disable SA1402 // File may only contain a single type - protocol DTOs are grouped

namespace ErikEJ.DacFX.TSQLAnalyzer.Protocol;

/// <summary>
/// Request sent from client to server via stdin.
/// </summary>
public sealed class ServerRequest
{
    /// <summary>
    /// Unique identifier for this request. Response will include same ID.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Command to execute: "analyze" or "shutdown".
    /// </summary>
    [JsonPropertyName("command")]
    public string Command { get; set; } = string.Empty;

    /// <summary>
    /// Path to .sql file or .dacpac to analyze (for analyze command).
    /// </summary>
    [JsonPropertyName("path")]
    public string? Path { get; set; }

    /// <summary>
    /// Rules configuration string, e.g., "Rules:-SRD0004;-SRN*".
    /// </summary>
    [JsonPropertyName("rules")]
    public string? Rules { get; set; }

    /// <summary>
    /// SQL Server version, e.g., "Sql160", "SqlAzure".
    /// </summary>
    [JsonPropertyName("sqlVersion")]
    public string? SqlVersion { get; set; }
}

/// <summary>
/// Response sent from server to client via stdout.
/// </summary>
public sealed class ServerResponse
{
    /// <summary>
    /// Request ID this response corresponds to.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Status: "success", "error", or "shutdown".
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Error message if status is "error".
    /// </summary>
    [JsonPropertyName("error")]
    public string? Error { get; set; }

    /// <summary>
    /// List of problems found during analysis.
    /// </summary>
    [JsonPropertyName("problems")]
    public IList<ServerProblem>? Problems { get; set; }
}

/// <summary>
/// Represents a single analysis problem.
/// </summary>
public sealed class ServerProblem
{
    /// <summary>
    /// Rule ID, e.g., "SqlServer.Rules.SRD0001".
    /// </summary>
    [JsonPropertyName("rule")]
    public string Rule { get; set; } = string.Empty;

    /// <summary>
    /// Line number where problem occurs.
    /// </summary>
    [JsonPropertyName("line")]
    public int Line { get; set; }

    /// <summary>
    /// Column number where problem occurs.
    /// </summary>
    [JsonPropertyName("column")]
    public int Column { get; set; }

    /// <summary>
    /// End line number where problem span ends.
    /// </summary>
    [JsonPropertyName("endLine")]
    public int EndLine { get; set; }

    /// <summary>
    /// End column number where problem span ends.
    /// </summary>
    [JsonPropertyName("endColumn")]
    public int EndColumn { get; set; }

    /// <summary>
    /// Problem description message.
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Documentation URL associated with this problem.
    /// </summary>
    [JsonPropertyName("helpLink")]
    public string? HelpLink { get; set; }

    /// <summary>
    /// File path where problem occurs.
    /// </summary>
    [JsonPropertyName("file")]
    public string? File { get; set; }
}

#pragma warning restore SA1402 // File may only contain a single type
#pragma warning restore SA1649 // File name should match first type name
