using Microsoft.SqlServer.Dac.Model;

namespace ErikEJ.DacFX.TSQLAnalyzer;

public class AnalyzerOptions
{
    /// <summary>
    /// Used to specify the scripts to analyze, can be a single file or a directory. Required.
    /// </summary>
#pragma warning disable CA1002 // Do not expose generic lists
    public List<string> Scripts { get; private set; } = [];
#pragma warning restore CA1002 // Do not expose generic lists

    /// <summary>
    /// Used to specify a rules expression similar to 'Rules:-SqlServer.Rules.SRD0010;+!SqlServer.Rules.SRN0005'. Optional.
    /// </summary>
    public string? Rules { get; set; }

    /// <summary>
    /// Used to specify the server release version, like 'Sql160' or 'SqlAzure' - defaults to 'Sql160'
    /// </summary>
    public SqlServerVersion SqlVersion { get; set; } = SqlServerVersion.Sql160;

    /// <summary>
    /// Used to specify the full filename for an optional output file.
    /// Supported format is 'xml'
    /// </summary>
    public FileInfo? OutputFile { get; set; }
}
