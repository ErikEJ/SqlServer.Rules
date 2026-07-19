using System.Text.Json.Serialization;

namespace ErikEJ.DacFX.TSQLAnalyzer.Protocol;

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
