using System.Text;
using Microsoft.SqlServer.Dac.CodeAnalysis;

namespace ErikEJ.DacFX.TSQLAnalyzer.Extensions;

/// <summary>
/// A wrapper for <see cref="SqlRuleProblem" /> that provides MSBuild compatible output and source document information.
/// </summary>
public static class SqlRuleProblemExtensions
{
    public static string GetOutputMessage(this SqlRuleProblem sqlRuleProblem, string? rules)
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

        var stringBuilder = new StringBuilder();
        stringBuilder.Append(sqlRuleProblem.SourceName);
        stringBuilder.Append('(');
        stringBuilder.Append(sqlRuleProblem.StartLine);
        stringBuilder.Append(',');
        stringBuilder.Append(sqlRuleProblem.StartColumn);
        stringBuilder.Append("):");
        stringBuilder.Append(' ');
        stringBuilder.Append(sqlRuleProblemSeverity);
        stringBuilder.Append(' ');
        stringBuilder.Append(sqlRuleProblem.ErrorMessageString);

        return stringBuilder.ToString();
    }
}
