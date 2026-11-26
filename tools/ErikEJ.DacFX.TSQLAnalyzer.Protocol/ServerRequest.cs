using System.Text.Json.Serialization;

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
    /// Path to .sql file or .dacpac to analyze. When content is supplied, this may identify the source file for configuration discovery.
    /// </summary>
    [JsonPropertyName("path")]
    public string? Path { get; set; }

    /// <summary>
    /// Raw SQL text to analyze in-memory. When set, takes precedence over <see cref="Path"/>.
    /// Enables analysis of unsaved editor buffer content (e.g. for a language server).
    /// </summary>
    [JsonPropertyName("content")]
    public string? Content { get; set; }

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

    /// <summary>
    /// Additional analyzer DLL paths to include in the analysis.
    /// </summary>
    [JsonPropertyName("additionalAnalyzers")]
#pragma warning disable CA2227 // Collection properties should be read only
    public IList<string>? AdditionalAnalyzers { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

    /// <summary>
    /// Optional analysis-wide EditorConfig file.
    /// </summary>
    [JsonPropertyName("editorConfigPath")]
    public string? EditorConfigPath { get; set; }
}
