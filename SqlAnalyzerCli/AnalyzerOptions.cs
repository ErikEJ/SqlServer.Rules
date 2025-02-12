using CommandLine;
using Microsoft.SqlServer.Dac.Model;

namespace ErikEJ.SqlAnalyzer;

internal sealed class AnalyzerOptions
{
    [Value(0, MetaName = "output", HelpText = "Output file name", Required = false)]

    [Option(
        'i',
        "input",
        HelpText = ".sql script file(s) to analyze",
        Required = true)]
    public IList<string> Scripts { get; set; } = [];

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
}
