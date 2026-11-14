using System.Text.Json.Serialization;

namespace ErikEJ.DacFX.TSQLAnalyzer.Protocol;

/// <summary>
/// An error that left part of the script unanalyzed, returned with a successful response.
/// </summary>
public sealed class ServerError
{
    /// <summary>
    /// "model" when a script could not be added to the model (for example a syntax error), so its objects were not
    /// analyzed; "analysis" when a rule failed while analyzing the model.
    /// </summary>
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = string.Empty;

    /// <summary>
    /// Error code, e.g. "SQL46010" for a model error, or the extensibility error code for an analysis error.
    /// </summary>
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    /// <summary>
    /// Error message.
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}
