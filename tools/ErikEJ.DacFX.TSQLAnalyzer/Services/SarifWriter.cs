using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using ErikEJ.DacFX.TSQLAnalyzer.Extensions;
using Microsoft.SqlServer.Dac.CodeAnalysis;

namespace ErikEJ.DacFX.TSQLAnalyzer.Services;

/// <summary>
/// Writes analysis problems as a SARIF 2.1.0 log, the format read by GitHub code scanning and Azure DevOps.
/// </summary>
internal static class SarifWriter
{
    private const string RulesPrefix = "Rules:";
    private const string ErrorRulePrefix = "+!";

    public static void Write(string path, AnalyzerResult result, IEnumerable<RuleDescriptor> rules, string? rulesExpression, string baseDirectory)
    {
        var descriptors = new Dictionary<string, RuleDescriptor>(StringComparer.OrdinalIgnoreCase);
        foreach (var rule in rules)
        {
            descriptors.TryAdd(rule.RuleId, rule);
        }

        var errorRules = ParseErrorRules(rulesExpression);
        var resolver = new SourceFileResolver();

        var findings = result.Problems
            .Select(problem =>
            {
                var (message, helpLink) = SplitHelpLink(problem.Description);
                return (Problem: problem, RuleId: problem.RuleId ?? "unknown", Message: message, HelpLink: helpLink ?? problem.GetMicrosoftRuleHelpLink());
            })
            .OrderBy(finding => finding.Problem.SourceName, StringComparer.Ordinal)
            .ThenBy(finding => finding.Problem.StartLine)
            .ThenBy(finding => finding.Problem.StartColumn)
            .ThenBy(finding => finding.RuleId, StringComparer.Ordinal)
            .ToList();

        var ruleIds = findings
            .Select(finding => finding.RuleId)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.Ordinal)
            .ToList();

        var ruleIndexes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < ruleIds.Count; index++)
        {
            ruleIndexes[ruleIds[index]] = index;
        }

        // Written without a byte order mark, which JSON parsers reject.
        using var stream = File.Create(path);
        using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });

        writer.WriteStartObject();
        writer.WriteString("$schema", "https://json.schemastore.org/sarif-2.1.0.json");
        writer.WriteString("version", "2.1.0");
        writer.WriteStartArray("runs");
        writer.WriteStartObject();

        writer.WriteStartObject("tool");
        writer.WriteStartObject("driver");
        writer.WriteString("name", "T-SQL Analyzer");
        writer.WriteString("informationUri", "https://github.com/ErikEJ/SqlServer.Rules");
        writer.WriteString("version", GetVersion());
        writer.WriteStartArray("rules");

        foreach (var ruleId in ruleIds)
        {
            descriptors.TryGetValue(ruleId, out var descriptor);
            var helpLink = findings.First(finding => string.Equals(finding.RuleId, ruleId, StringComparison.OrdinalIgnoreCase)).HelpLink;
            WriteRule(writer, ruleId, descriptor, helpLink);
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
        writer.WriteEndObject();

        writer.WriteString("columnKind", "utf16CodeUnits");
        writer.WriteStartArray("results");

        foreach (var (problem, ruleId, message, _) in findings)
        {
            writer.WriteStartObject();
            writer.WriteString("ruleId", ruleId);
            writer.WriteNumber("ruleIndex", ruleIndexes[ruleId]);
            writer.WriteString("level", IsError(problem, ruleId, errorRules) ? "error" : "warning");
            writer.WriteStartObject("message");
            writer.WriteString("text", message);
            writer.WriteEndObject();
            WriteLocations(writer, problem, result, resolver, baseDirectory);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    /// <summary>
    /// Gets the URI of an analyzed file: relative to the base directory when the file is below it, so that the log
    /// does not depend on where the repository is checked out, and an absolute file URI otherwise.
    /// </summary>
    /// <param name="fullPath">The full path of the analyzed file.</param>
    /// <param name="baseDirectory">The directory that relative URIs start from.</param>
    /// <returns>The artifact URI.</returns>
    internal static string GetArtifactUri(string fullPath, string baseDirectory)
    {
        var relative = Path.GetRelativePath(baseDirectory, fullPath);
        var segments = relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        if (Path.IsPathRooted(relative) || segments[0] == "..")
        {
            return new Uri(fullPath).AbsoluteUri;
        }

        return string.Join('/', segments.Select(Uri.EscapeDataString));
    }

    private static void WriteRule(Utf8JsonWriter writer, string ruleId, RuleDescriptor? descriptor, string? helpLink)
    {
        var name = string.IsNullOrWhiteSpace(descriptor?.DisplayName) ? ruleId : descriptor.DisplayName;
        var description = string.IsNullOrWhiteSpace(descriptor?.DisplayDescription) ? name : descriptor.DisplayDescription;

        writer.WriteStartObject();
        writer.WriteString("id", ruleId);
        writer.WriteStartObject("shortDescription");
        writer.WriteString("text", name);
        writer.WriteEndObject();
        writer.WriteStartObject("fullDescription");
        writer.WriteString("text", description);
        writer.WriteEndObject();

        if (helpLink != null)
        {
            writer.WriteString("helpUri", helpLink);
        }

        writer.WriteStartObject("help");
        writer.WriteString("text", helpLink == null ? description : $"{description} {helpLink}");
        writer.WriteEndObject();
        writer.WriteEndObject();
    }

    private static void WriteLocations(Utf8JsonWriter writer, SqlRuleProblem problem, AnalyzerResult result, SourceFileResolver resolver, string baseDirectory)
    {
        writer.WriteStartArray("locations");

        var filePath = resolver.GetFilePath(problem.SourceName);

        if (filePath != null)
        {
            writer.WriteStartObject();
            writer.WriteStartObject("physicalLocation");
            writer.WriteStartObject("artifactLocation");
            writer.WriteString("uri", GetArtifactUri(filePath, baseDirectory));
            writer.WriteEndObject();
            WriteRegion(writer, problem, result);
            writer.WriteEndObject();
            writer.WriteEndObject();
        }
        else if (!string.IsNullOrEmpty(problem.SourceName) && problem.SourceName != "-1")
        {
            // Problems from a .dacpac or a live database are located by object name; their line numbers refer to
            // the object's script, which is not a file.
            writer.WriteStartObject();
            writer.WriteStartArray("logicalLocations");
            writer.WriteStartObject();
            writer.WriteString("fullyQualifiedName", problem.SourceName);
            writer.WriteEndObject();
            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }

    private static void WriteRegion(Utf8JsonWriter writer, SqlRuleProblem problem, AnalyzerResult result)
    {
        // Model-level problems are reported at line 0; SARIF lines and columns are 1-based.
        if (problem.StartLine < 1)
        {
            return;
        }

        writer.WriteStartObject("region");
        writer.WriteNumber("startLine", problem.StartLine);

        var startColumn = result.GetAdjustedColumn(problem.StartLine, problem.StartColumn, problem.SourceName);

        if (startColumn > 0)
        {
            writer.WriteNumber("startColumn", startColumn);

            var (endLine, endColumn) = problem.GetEndPosition(result);

            if (endLine > problem.StartLine || (endLine == problem.StartLine && endColumn > startColumn))
            {
                writer.WriteNumber("endLine", endLine);
                writer.WriteNumber("endColumn", endColumn);
            }
        }

        writer.WriteEndObject();
    }

    private static string GetVersion()
    {
        var version = typeof(SarifWriter).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "1.0.0";
        var metadata = version.IndexOf('+', StringComparison.Ordinal);

        return metadata < 0 ? version : version[..metadata];
    }

    // Rule descriptions end with the documentation link in parentheses; SARIF carries it as the rule's help URI.
    private static (string Message, string? HelpLink) SplitHelpLink(string? description)
    {
        var trimmed = description?.Trim() ?? string.Empty;
        var linkStart = trimmed.LastIndexOf(" (http", StringComparison.OrdinalIgnoreCase);

        if (linkStart < 0 || !trimmed.EndsWith(')'))
        {
            return (trimmed, null);
        }

        var link = trimmed[(linkStart + 2)..^1];

        return Uri.TryCreate(link, UriKind.Absolute, out _)
            ? (trimmed[..linkStart].TrimEnd(), link)
            : (trimmed, null);
    }

    // Rules promoted to errors with "+!" in the rules expression, as in SQL projects.
    private static HashSet<string> ParseErrorRules(string? rulesExpression)
    {
        var errorRules = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(rulesExpression))
        {
            return errorRules;
        }

        var expression = rulesExpression.Trim();
        if (expression.StartsWith(RulesPrefix, StringComparison.OrdinalIgnoreCase))
        {
            expression = expression[RulesPrefix.Length..];
        }

        foreach (var rule in expression
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(rule => rule.StartsWith(ErrorRulePrefix, StringComparison.Ordinal) && rule.Length > ErrorRulePrefix.Length))
        {
            errorRules.Add(rule[ErrorRulePrefix.Length..]);
        }

        return errorRules;
    }

    private static bool IsError(SqlRuleProblem problem, string ruleId, HashSet<string> errorRules)
        => problem.Severity == SqlRuleProblemSeverity.Error
            || errorRules.Contains(ruleId)
            || errorRules.Any(rule => rule.EndsWith('*') && ruleId.StartsWith(rule[..^1], StringComparison.OrdinalIgnoreCase));
}
