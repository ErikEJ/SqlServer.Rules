using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;

namespace ErikEJ.DacFX.TSQLAnalyzer;

public class AnalyzerResult
{
    public CodeAnalysisResult? Result { get; internal set; }

    public Dictionary<string, DacModelException> ModelErrors { get; } = [];
}
