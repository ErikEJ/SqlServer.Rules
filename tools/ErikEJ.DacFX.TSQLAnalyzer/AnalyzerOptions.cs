using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Dac.Model;

namespace ErikEJ.DacFX.TSQLAnalyzer;

public class AnalyzerOptions
{
    /// <summary>
    /// Used to specify the scripts to analyze, can be a single file or a directory or a wildcard. Required if connection string is not set.
    /// </summary>
#pragma warning disable CA2227 // Collection properties should be read only
#pragma warning disable CA1002 // Do not expose generic lists
    public List<string>? Scripts { get; set; } = [];

    /// <summary>
    /// Used to specify additional analyzers to include in the analysis.
    /// </summary>
    public List<string>? AdditionalAnalyzers { get; set; } = [];
#pragma warning restore CA1002 // Do not expose generic lists

    /// <summary>
    /// Used to specify the connection string to a SQL Server database. Required if scripts is not set.
    /// </summary>
    public SqlConnectionStringBuilder? ConnectionString { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

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

    /// <summary>
    /// Used to specify if formatting of the .sql files selected for analysis is required.
    /// </summary>
    public bool Format { get; set; }
}
