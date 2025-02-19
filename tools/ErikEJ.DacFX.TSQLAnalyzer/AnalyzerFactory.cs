using ErikEJ.DacFX.TSQLAnalyzer.Services;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;

namespace ErikEJ.DacFX.TSQLAnalyzer;

public class AnalyzerFactory
{
    private readonly SqlFileCollector sqlFileCollector = new();
    private readonly HashSet<string> ignoredRules = new();
    private readonly HashSet<string> ignoredRuleSets = new();
    private readonly AnalyzerOptions request;

    public AnalyzerFactory(AnalyzerOptions analyzerOptions)
    {
        request = analyzerOptions;
    }

    public AnalyzerResult Analyze()
    {
        var result = new AnalyzerResult();

        if (!string.IsNullOrWhiteSpace(request.Rules))
        {
            BuildRuleLists(request.Rules);
        }

        if (request.Scripts.Count == 0)
        {
            throw new ArgumentException("No files to analyze");
        }

        var files = sqlFileCollector.ProcessList(request.Scripts);

        if (files.Count == 0)
        {
            throw new ArgumentException("No files found to analyze");
        }

        using var model = new TSqlModel(request.SqlVersion, new TSqlModelOptions());

        var filesAdded = 0;

        foreach (var (fileName, fileContents) in files)
        {
            try
            {
                model.AddOrUpdateObjects(fileContents, fileName, new TSqlObjectOptions());
                filesAdded++;
            }
            catch (DacModelException dex)
            {
                result.ModelErrors.Add(fileName, dex);
            }
        }

        var factory = new CodeAnalysisServiceFactory();
        var service = factory.CreateAnalysisService(model);

        if (ignoredRules.Count > 0
                    || ignoredRuleSets.Count > 0)
        {
            service.SetProblemSuppressor(p => ignoredRules.Contains(p.Rule.RuleId)
                || ignoredRuleSets.Any(s => p.Rule.RuleId.StartsWith(s, StringComparison.OrdinalIgnoreCase)));
        }

        var analysisResult = service.Analyze(model);

        if (analysisResult == null)
        {
            throw new ArgumentException("Analysis failed");
        }

        if (analysisResult.AnalysisSucceeded)
        {
            result.Result = analysisResult;
        }

        return result;
    }

    private void BuildRuleLists(string rulesExpression)
    {
        char[] separator = [';'];
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
        }
    }
}