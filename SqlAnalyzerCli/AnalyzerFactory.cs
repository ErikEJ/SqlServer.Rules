using System.Diagnostics;
using System.Globalization;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Spectre.Console;
using SqlAnalyzerCli.Services;

namespace ErikEJ.SqlAnalyzer;

internal sealed class AnalyzerFactory
{
    private readonly SqlFileCollector sqlFileCollector = new();
    private readonly HashSet<string> ignoredRules = new();
    private readonly HashSet<string> ignoredRuleSets = new();
    private readonly HashSet<string> errorRuleSets = new();
    private readonly char[] separator = [';'];

    public int Create(AnalyzerOptions request)
    {
        var sw = Stopwatch.StartNew();

        if (!string.IsNullOrWhiteSpace(request.Rules))
        {
            BuildRuleLists(request.Rules);
        }

        SendNotification($"Loading files", Color.Default);

        var files = sqlFileCollector.ProcessList(request.Scripts);

        if (files.Count == 0)
        {
            DisplayService.Error("No files found to analyze");
            return 1;
        }

        using var model = new TSqlModel(request.SqlVersion, new TSqlModelOptions());

        foreach (var (fileName, fileContents) in files)
        {
            model.AddOrUpdateObjects(fileContents, fileName, new TSqlObjectOptions());
        }

        var factory = new CodeAnalysisServiceFactory();
        var service = factory.CreateAnalysisService(model);

        if (ignoredRules.Count > 0
                    || ignoredRuleSets.Count > 0)
        {
            service.SetProblemSuppressor(p => ignoredRules.Contains(p.Rule.RuleId)
                || ignoredRuleSets.Any(s => p.Rule.RuleId.StartsWith(s, StringComparison.OrdinalIgnoreCase)));
        }

        sw.Stop();
        SendNotification($"Loading files completed in: {sw.Elapsed.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture)}", Color.Default);

        sw = Stopwatch.StartNew();

        var result = DisplayService.Wait(
            "Analyzing scripts...",
            () => service.Analyze(model));

        sw.Stop();

        if (result == null)
        {
            DisplayService.Error("Analysis failed");
            return 1;
        }

        foreach (var err in result.InitializationErrors)
        {
            DisplayService.Error(err.Message);
        }

        foreach (var err in result.SuppressionErrors)
        {
            DisplayService.Error(err.Message);
        }

        foreach (var err in result.AnalysisErrors)
        {
            DisplayService.Error(err.Message);
        }

        if (result.AnalysisSucceeded)
        {
            foreach (var err in result.Problems)
            {
                var warning = err.GetOutputMessage(errorRuleSets);

                DisplayService.MarkupLine(
                () => DisplayService.Markup("warning:", Color.Yellow),
                () => DisplayService.Markup(
                    warning
                    .Replace("[", "[[", StringComparison.OrdinalIgnoreCase)
                    .Replace("]", "]]", StringComparison.OrdinalIgnoreCase),
                    Decoration.None));
            }

            SendNotification($"Analysis completed in: {sw.Elapsed.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture)} with {result.Problems.Count} problems", Color.Default);

            return 0;
        }

        DisplayService.Error($"Analysis failed");

        return 1;
    }

    private void SendNotification(string message, Color color)
    {
        DisplayService.MarkupLine(string.Empty, color);
        DisplayService.MarkupLine(message, color);
    }

    private void BuildRuleLists(string rulesExpression)
    {
        rulesExpression = rulesExpression.Remove(0, 6);

        if (!string.IsNullOrWhiteSpace(rulesExpression))
        {
            foreach (var rule in rulesExpression.Split(
                separator,
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Where(rule => rule
                        .StartsWith('-')
                            && rule.Length > 1))
            {
                if (rule.Length > 2 && rule.EndsWith('*'))
                {
                    ignoredRuleSets.Add(rule[1..^1]);
                }
                else
                {
                    ignoredRules.Add(rule[1..]);
                }
            }

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
    }
}