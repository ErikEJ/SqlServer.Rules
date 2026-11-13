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
#pragma warning disable CA2227 // Collection properties should be read only
    public IList<ServerProblem>? Problems { get; set; }

    /// <summary>
    /// Errors that left part of the script unanalyzed, when status is "success": scripts that could not be added to the
    /// model and rules that failed. Problems are then incomplete. Null when there are none.
    /// </summary>
    [JsonPropertyName("errors")]
    public IList<ServerError>? Errors { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
}
