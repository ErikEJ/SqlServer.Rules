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

        var link = string.Empty;

        if (sqlRuleProblem.ShortRuleId.StartsWith("SR00", StringComparison.Ordinal))
        {
            if (RulesInfo.MicrosoftRules.TryGetValue(sqlRuleProblem.ShortRuleId, out var ruleInfo))
            {
                link = $" ({ruleInfo.Item1})";
            }
        }

        var stringBuilder = new StringBuilder();
        stringBuilder.Append(sqlRuleProblem.SourceName);
        stringBuilder.Append('(');
        stringBuilder.Append(sqlRuleProblem.StartLine);
        stringBuilder.Append(',');
        stringBuilder.Append(sqlRuleProblem.StartColumn);
        stringBuilder.Append("):");
        stringBuilder.Append(' ');
        stringBuilder.Append(CultureInfo.InvariantCulture, $"{sqlRuleProblem.RuleId} : {sqlRuleProblem.Description}{link}");

        return stringBuilder.ToString();
    }
}
