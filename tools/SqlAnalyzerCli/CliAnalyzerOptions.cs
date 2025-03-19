using CommandLine;
using Microsoft.SqlServer.Dac.Model;

namespace ErikEJ.SqlAnalyzer;

internal sealed class CliAnalyzerOptions
{
    [Value(0, MetaName = "output", HelpText = "Output file name", Required = false)]

    [Option(
        'i',
        "input",
        HelpText = ".sql script file(s) to analyze - if not supplied, assumes all .sql files under current directory.",
        Required = false)]
    public IList<string>? Scripts { get; set; } = [];

    [Option(
        'c',
        "connectionstring",
        HelpText = "Connection string of the database to analyze",
        Required = false)]
    public string? ConnectionString { get; set; }

    [Option(
        'r',
        longName: "rules",
        Required = false,
        HelpText = "Used to specify a rules expression similar to 'Rules:-SqlServer.Rules.SRD0010;+!SqlServer.Rules.SRN0005'")]
    public string Rules { get; set; } = string.Empty;

    [Option(
        's',
        longName: "sqlversion",
        Required = false,
        HelpText = "Used to specify the server release version, like 'Sql160' or 'SqlAzure' - defaults to 'Sql160'")]
    public SqlServerVersion SqlVersion { get; set; } = SqlServerVersion.Sql160;

    [Option(
    'n',
    longName: "nologo",
    Required = false,
    HelpText = "Compact output without logo and informational messages")]
    public bool NoLogo { get; set; }

    [Option(
        'o',
        longName: "output",
        Required = false,
        HelpText = "Optional file name of output file in .xml format")]
    public string? OutputFile { get; set; }

    [Option(
        'a',
        "analyzers",
        HelpText = "Directory path of additional analyzer .dll files, can be specified multiple times.",
        Required = false)]
    public IList<string>? AdditionalAnalyzers { get; set; } = [];
}
