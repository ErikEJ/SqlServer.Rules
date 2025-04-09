using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;

namespace ErikEJ.DacFX.TSQLAnalyzer;

public class AnalyzerResult
{
    public CodeAnalysisResult? Result { get; internal set; }

    public Dictionary<string, DacModelException> ModelErrors { get; } = [];

#pragma warning disable CA1002 // Do not expose generic lists
    public List<string> FormattedFiles { get; } = [];
#pragma warning restore CA1002 // Do not expose generic lists

    public int FileCount { get; internal set; }

    public string? OutputFile { get; internal set; }

    public string? Analyzers { get; internal set; }
}
