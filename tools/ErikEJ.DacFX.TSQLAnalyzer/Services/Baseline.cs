using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.SqlServer.Dac.CodeAnalysis;

namespace ErikEJ.DacFX.TSQLAnalyzer.Services;

/// <summary>
/// Accepted problems, stored as a count per source and rule. Counts rather than positions keep the baseline valid
/// when lines shift or rule messages change.
/// </summary>
internal sealed class Baseline
{
    private const int CurrentVersion = 1;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private readonly string baseDirectory;
    private readonly SourceFileResolver resolver = new();
    private readonly Dictionary<string, Dictionary<string, int>> counts;

    private Baseline(string fullPath, Dictionary<string, Dictionary<string, int>> counts)
    {
        baseDirectory = Path.GetDirectoryName(fullPath) ?? Directory.GetCurrentDirectory();
        this.counts = counts;
    }

    public static Baseline Load(string path)
    {
        var fullPath = Path.GetFullPath(path);

        if (!File.Exists(fullPath))
        {
            throw new ArgumentException($"Baseline file '{fullPath}' was not found. Use --update-baseline to create it.");
        }

        try
        {
            var document = JsonSerializer.Deserialize<Document>(File.ReadAllText(fullPath), JsonOptions);

            if (document?.Problems == null || document.Version != CurrentVersion)
            {
                throw new FormatException($"Expected a version {CurrentVersion} baseline.");
            }

            var counts = NewCounts();

            foreach (var (source, rules) in document.Problems)
            {
                foreach (var (rule, count) in rules)
                {
                    Add(counts, source, rule, count);
                }
            }

            return new Baseline(fullPath, counts);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException or FormatException)
        {
            throw new ArgumentException($"Unable to load baseline file '{fullPath}': {exception.Message}", exception);
        }
    }

    public static void Save(string path, IEnumerable<SqlRuleProblem> problems)
    {
        var fullPath = Path.GetFullPath(path);
        var baseline = new Baseline(fullPath, NewCounts());

        foreach (var problem in problems)
        {
            Add(baseline.counts, baseline.GetSource(problem), GetRule(problem), 1);
        }

        // Sorted so that an updated baseline produces a minimal diff.
        var document = new Document { Version = CurrentVersion };
        foreach (var (source, rules) in baseline.counts)
        {
            document.Problems[source] = new SortedDictionary<string, int>(rules, StringComparer.Ordinal);
        }

        try
        {
            File.WriteAllText(fullPath, JsonSerializer.Serialize(document, JsonOptions));
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            throw new ArgumentException($"Unable to write baseline file '{fullPath}': {exception.Message}", exception);
        }
    }

    /// <summary>
    /// Gets the problems that the baseline does not cover. When a source has more problems for a rule than the
    /// baseline accepts, all of them are returned, because the new ones cannot be told apart from the accepted ones.
    /// </summary>
    /// <param name="problems">The problems found by the analysis.</param>
    /// <returns>The problems to report.</returns>
    public IReadOnlyList<SqlRuleProblem> GetNewProblems(IEnumerable<SqlRuleProblem> problems)
    {
        var keyed = problems
            .Select(problem => (Problem: problem, Source: GetSource(problem), Rule: GetRule(problem)))
            .ToList();

        var found = NewCounts();
        foreach (var (_, source, rule) in keyed)
        {
            Add(found, source, rule, 1);
        }

        return keyed
            .Where(key => found[key.Source][key.Rule] > GetCount(key.Source, key.Rule))
            .Select(key => key.Problem)
            .ToList();
    }

    private static Dictionary<string, Dictionary<string, int>> NewCounts()
        => new(StringComparer.Ordinal);

    private static void Add(Dictionary<string, Dictionary<string, int>> counts, string source, string rule, int count)
    {
        if (!counts.TryGetValue(source, out var rules))
        {
            rules = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            counts[source] = rules;
        }

        rules[rule] = rules.GetValueOrDefault(rule) + count;
    }

    private static string GetRule(SqlRuleProblem problem)
        => problem.RuleId ?? string.Empty;

    private int GetCount(string source, string rule)
        => counts.TryGetValue(source, out var rules) ? rules.GetValueOrDefault(rule) : 0;

    // Files are keyed relative to the baseline file, so that a baseline committed to a repository matches on any
    // machine. Database object names are used as they are.
    private string GetSource(SqlRuleProblem problem)
    {
        var filePath = resolver.GetFilePath(problem.SourceName);

        if (filePath != null)
        {
            return Path.GetRelativePath(baseDirectory, filePath).Replace('\\', '/');
        }

        return problem.SourceName is null or "-1" ? string.Empty : problem.SourceName;
    }

    private sealed class Document
    {
        public int Version { get; set; }

        public SortedDictionary<string, SortedDictionary<string, int>> Problems { get; set; } = new(StringComparer.Ordinal);
    }
}
