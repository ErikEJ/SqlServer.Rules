using System.Globalization;
using System.Text;
using Microsoft.SqlServer.Dac.CodeAnalysis;

namespace ErikEJ.DacFX.TSQLAnalyzer.Extensions;

/// <summary>
/// A wrapper for <see cref="SqlRuleProblem" /> that provides MSBuild compatible output and source document information.
/// </summary>
public static class SqlRuleProblemExtensions
{
    public static string GetOutputMessage(this SqlRuleProblem sqlRuleProblem, string? rules)
        => sqlRuleProblem.GetOutputMessage(rules, null);

    public static string GetOutputMessage(this SqlRuleProblem sqlRuleProblem, string? rules, AnalyzerResult? analyzerResult)
    {
        ArgumentNullException.ThrowIfNull(sqlRuleProblem);

        HashSet<string> errorRuleSets = new();
        char[] separator = [';'];

        if (!string.IsNullOrEmpty(rules) && rules.Length > 6)
        {
            var rulesExpression = rules.Remove(0, 6);

            foreach (var rule in rulesExpression.Split(
                separator,
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Where(rule => rule
                        .StartsWith("+!", StringComparison.OrdinalIgnoreCase)
                            && rule.Length > 2))
                {
                    errorRuleSets.Add(rule[2..]);
                }
        }

        SqlRuleProblemSeverity sqlRuleProblemSeverity = sqlRuleProblem.Severity;

        if (errorRuleSets.Contains(sqlRuleProblem.RuleId))
        {
            sqlRuleProblemSeverity = SqlRuleProblemSeverity.Error;
        }

        var wildCardErrorRules = errorRuleSets
            .Where(r => r.EndsWith('*'));
        if (wildCardErrorRules.Any(s => sqlRuleProblem.RuleId.StartsWith(s[..^1], StringComparison.OrdinalIgnoreCase)))
        {
            sqlRuleProblemSeverity = SqlRuleProblemSeverity.Error;
        }

        var helpLink = sqlRuleProblem.GetMicrosoftRuleHelpLink();
        var link = helpLink is null ? string.Empty : $" ({helpLink})";

        var startColumn = analyzerResult?.GetAdjustedColumn(sqlRuleProblem.StartLine, sqlRuleProblem.StartColumn, sqlRuleProblem.SourceName)
                          ?? sqlRuleProblem.StartColumn;

        var (endLine, endColumn) = sqlRuleProblem.GetEndPosition(analyzerResult);

        var stringBuilder = new StringBuilder();
        stringBuilder.Append(sqlRuleProblem.SourceName);
        stringBuilder.Append('(');
        stringBuilder.Append(sqlRuleProblem.StartLine);
        stringBuilder.Append(',');
        stringBuilder.Append(startColumn);
        stringBuilder.Append(',');
        stringBuilder.Append(endLine);
        stringBuilder.Append(',');
        stringBuilder.Append(endColumn);
        stringBuilder.Append("):");
        stringBuilder.Append(' ');
        stringBuilder.Append(CultureInfo.InvariantCulture, $"{sqlRuleProblem.RuleId} : {sqlRuleProblem.Description}{link}");

        return stringBuilder.ToString();
    }

    /// <summary>
    /// Gets the Microsoft Learn help link for a built-in DacFx rule (ids starting with <c>SR00</c>).
    /// Returns <see langword="null" /> for rules that are not Microsoft rules or have no known link.
    /// </summary>
    /// <param name="sqlRuleProblem">The analyzed problem.</param>
    /// <returns>The help link URL, or <see langword="null" /> when none is available.</returns>
    public static string? GetMicrosoftRuleHelpLink(this SqlRuleProblem sqlRuleProblem)
    {
        ArgumentNullException.ThrowIfNull(sqlRuleProblem);

        if (sqlRuleProblem.ShortRuleId.StartsWith("SR00", StringComparison.Ordinal)
            && RulesInfo.MicrosoftRules.TryGetValue(sqlRuleProblem.ShortRuleId, out var ruleInfo))
        {
            return ruleInfo.Item1;
        }

        return null;
    }

    public static (int EndLine, int EndColumn) GetEndPosition(this SqlRuleProblem sqlRuleProblem)
        => sqlRuleProblem.GetEndPosition(null);

    public static (int EndLine, int EndColumn) GetEndPosition(this SqlRuleProblem sqlRuleProblem, AnalyzerResult? analyzerResult)
    {
        ArgumentNullException.ThrowIfNull(sqlRuleProblem);

        var endLine = sqlRuleProblem.StartLine;
        var endColumn = sqlRuleProblem.StartColumn;

        var fragment = sqlRuleProblem.Fragment;
        var tokens = fragment?.ScriptTokenStream;

        if (fragment != null && tokens != null && tokens.Count > 0 &&
            fragment.LastTokenIndex >= 0 && fragment.LastTokenIndex < tokens.Count)
        {
            var lastToken = tokens[fragment.LastTokenIndex];
            if (lastToken != null)
            {
                endLine = lastToken.Line;
                endColumn = lastToken.Column + (lastToken.Text?.Length ?? 0);
            }
        }

        if (analyzerResult != null)
        {
            endColumn = analyzerResult.GetAdjustedColumn(endLine, endColumn, sqlRuleProblem.SourceName);
        }

        return (endLine, endColumn);
    }
}
