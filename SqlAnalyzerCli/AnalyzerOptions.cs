using CommandLine;

namespace ErikEJ.SqlAnalyzer;

internal sealed class AnalyzerOptions
{
    [Option(
        'i',
        "input",
        HelpText = ".sql script file(s) to analyze",
        Required = true)]
    public IEnumerable<string> Scripts { get; set; } = [];

    [Option(
        'r',
        longName: "rules",
        Required = false,
        HelpText = "Used to specify a rules expression similar to '-SqlServer.Rules.SRD0010;+!SqlServer.Rules.SRN0005'")]
    public string Rules { get; set; } = string.Empty;

    // TODO Add SqlVersion option
}
