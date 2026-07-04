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
    /// Per-file column adjustments produced by <see cref="Services.BatchWrapper"/>.
    /// The key is the source file name (as passed to the model). Each adjustment says that for the
    /// given line, any reported column at or after <see cref="ColumnAdjustment.StartColumn"/> should
    /// be shifted left by <see cref="ColumnAdjustment.Delta"/> characters to map back to the
    /// original source text.
    /// </summary>
    internal Dictionary<string, IReadOnlyList<ColumnAdjustment>> ColumnAdjustmentsByFile { get; } = [];

    /// <summary>
    /// Returns the original-source column for a position in the wrapped script.
    /// When the line was shifted by preprocessing (for example an ad-hoc wrapper prefix or ALTER to
    /// CREATE normalization), the matching adjustment delta is subtracted; otherwise the column is
    /// returned as-is.
    /// </summary>
    /// <param name="line">The 1-based line number as reported by the analysis engine.</param>
    /// <param name="column">The 1-based column number as reported by the analysis engine.</param>
    /// <param name="sourceName">The source file name as recorded by the model, or <c>null</c> for script-string inputs.</param>
    /// <returns>The original source column, adjusted for any wrapper prefix that was inserted on the same line.</returns>
    public int GetAdjustedColumn(int line, int column, string? sourceName)
    {
        var key = sourceName ?? string.Empty;
        if (!ColumnAdjustmentsByFile.TryGetValue(key, out var adjustments))
        {
            // DacFx assigns an internal default source name (e.g. "-1") to objects added via
            // AddObjects without a named source. Fall back to the empty-string sentinel used for
            // script-string inputs so that column adjustments are still applied in that case.
            if (key.Length == 0 || !ColumnAdjustmentsByFile.TryGetValue(string.Empty, out adjustments))
            {
                return column;
            }
        }

        foreach (var adjustment in adjustments
            .Where(a => a.Line == line && column >= a.StartColumn)
            .OrderByDescending(a => a.StartColumn))
        {
            column -= adjustment.Delta;
        }

        return column;
    }
}
