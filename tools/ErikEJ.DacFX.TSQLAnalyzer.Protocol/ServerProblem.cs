using System.Text.Json.Serialization;

namespace ErikEJ.DacFX.TSQLAnalyzer.Protocol;

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
    /// Severity of the problem, e.g., "Warning" or "Error".
    /// </summary>
    [JsonPropertyName("severity")]
    public string Severity { get; set; } = string.Empty;

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
