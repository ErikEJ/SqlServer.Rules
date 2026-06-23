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

    /// <summary>
    /// Per-file column-offset adjustments produced by <see cref="Services.BatchWrapper"/>.
    /// Outer key is the source file name (as passed to the model). Inner dictionary maps each
    /// line number to the number of characters prepended on that line by the synthetic-procedure
    /// wrapper prefix.
    /// </summary>
    internal Dictionary<string, IReadOnlyDictionary<int, int>> ColumnAdjustmentsByFile { get; } = [];

    /// <summary>
    /// Returns the original-source column for a position in the wrapped script.
    /// When the line was shifted by a synthetic-procedure prefix the prefix length is subtracted;
    /// otherwise the column is returned as-is.
    /// </summary>
    /// <param name="line">The 1-based line number as reported by the analysis engine.</param>
    /// <param name="column">The 1-based column number as reported by the analysis engine.</param>
    /// <param name="sourceName">The source file name as recorded by the model, or <c>null</c> for script-string inputs.</param>
    /// <returns>The original source column, adjusted for any wrapper prefix that was inserted on the same line.</returns>
    public int GetAdjustedColumn(int line, int column, string? sourceName)
    {
        var key = sourceName ?? string.Empty;
        if (ColumnAdjustmentsByFile.TryGetValue(key, out var adjustments)
            && adjustments.TryGetValue(line, out var prefixLength))
        {
            return column - prefixLength;
        }

        return column;
    }
}
