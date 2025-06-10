using System.IO.Compression;
using System.Text;
using System.Text.Json;
using ErikEJ.DacFX.TSQLAnalyzer.Services;
using Microsoft.SqlServer.Dac;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;

namespace ErikEJ.DacFX.TSQLAnalyzer;

public class AnalyzerFactory
{
    private readonly SqlFileCollector sqlFileCollector = new();
    private readonly HashSet<string> ignoredRules = new();
    private readonly HashSet<string> ignoredRuleSets = new();
    private readonly AnalyzerOptions request;
    private Dictionary<string, string> files = [];

    public AnalyzerFactory(AnalyzerOptions analyzerOptions)
    {
        request = analyzerOptions;
    }

    public AnalyzerResult Analyze()
    {
        var result = new AnalyzerResult();

        if ((request.Scripts == null || request.Scripts.Count == 0) && request.ConnectionString == null && string.IsNullOrEmpty(request.Script))
        {
            throw new ArgumentException("No script paths, script body or connection string specified");
        }

        if (!string.IsNullOrWhiteSpace(request.Rules))
        {
            BuildRuleLists(request.Rules);
        }

        using var model = GenerateTSqlModel(result);

        if (model == null)
        {
            throw new ArgumentException("Model creation failed");
        }

        var settings = new CodeAnalysisServiceSettings();

        if (request.AdditionalAnalyzers?.Count > 0)
        {
            settings.AssemblyLookupPath = string.Join(';', request.AdditionalAnalyzers.Distinct());
        }

        var factory = new CodeAnalysisServiceFactory();
        var service = factory.CreateAnalysisService(model, settings);

        var rules = service.GetRules();

        result.Analyzers = string.Join(", ", rules.Select(a => a.Namespace).Distinct());

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

        result.Result = analysisResult;

        if (analysisResult.AnalysisSucceeded)
        {
            if (request.OutputFile != null)
            {
                SaveOutputFile(result, analysisResult);
            }

            if (request.Format)
            {
                Format(files, result);
            }
        }

        return result;
    }

    private void SaveOutputFile(AnalyzerResult result, CodeAnalysisResult analysisResult)
    {
        var outputFile = GetOutputFile(request.OutputFile);

        if (outputFile != null)
        {
            if (outputFile.Exists)
            {
                outputFile.Delete();
            }

            if (outputFile.Extension.Equals(".xml", StringComparison.OrdinalIgnoreCase))
            {
                analysisResult.SerializeResultsToXml(outputFile.FullName);

                result.OutputFile = outputFile.FullName;
            }

            if (outputFile.Extension.Equals(".json", StringComparison.OrdinalIgnoreCase))
            {
                var problemList = new ProblemList { Problems = new List<PlainProblem>() };

                foreach (var problem in analysisResult.Problems)
                {
                    problemList.Problems.Add(new PlainProblem
                    {
                        Column = problem.StartColumn,
                        Line = problem.StartLine,
                        Description = problem.Description,
                        Rule = problem.RuleId,
                        Severity = problem.Severity.ToString(),
                        SourceFile = problem.SourceName,
                    });
                }

                File.WriteAllText(outputFile.FullName, JsonSerializer.Serialize(problemList));
            }
        }
    }

    private void Format(Dictionary<string, string> files, AnalyzerResult result)
    {
        var formatter = new Formatter();

        var formattedFiles = new List<string>();

        foreach (var file in files)
        {
            var formatted = Formatter.Format(file.Value, file.Key);

            if (formatted.Completed)
            {
                File.WriteAllText(file.Key, formatted.FormattedText, Encoding.UTF8);
                result.FormattedFiles.Add(file.Key);
            }
        }
    }

    private TSqlModel GenerateTSqlModel(AnalyzerResult result)
    {
        var model = new TSqlModel(request.SqlVersion, new TSqlModelOptions());

        if (request.Scripts != null && request.Scripts.Count > 0 && request.ConnectionString == null)
        {
            var files = sqlFileCollector.ProcessList(request.Scripts);

            if (files.Count == 1 && files.First().Key.EndsWith(".dacpac", StringComparison.OrdinalIgnoreCase))
            {
                model = CreateDacpacModel(files.First().Key);
            }
            else if (files.Count == 1 && files.First().Key.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                var zipFile = files.First().Key;
                var targetDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                ZipFile.ExtractToDirectory(zipFile, targetDirectory);

                files = sqlFileCollector.ProcessList([targetDirectory]);

                AddFilesToModel(result, model, files);
            }
            else
            {
                this.files = files;
                AddFilesToModel(result, model, files);
            }
        }
        else if (request.ConnectionString != null)
        {
            var dacpacExtractor = new DacpacExtractor(request.ConnectionString);
            var dbDacpac = dacpacExtractor.ExtractDacpac();

            model = CreateDacpacModel(dbDacpac.FullName);
        }
        else if (!string.IsNullOrWhiteSpace(request.Script))
        {
            AddScriptToModel(result, model, request.Script);
        }

        return model;
    }

    private static void AddFilesToModel(AnalyzerResult result, TSqlModel model, Dictionary<string, string> files)
    {
        if (files.Count == 0)
        {
            throw new ArgumentException("No files found to analyze");
        }

        result.FileCount = files.Count;

        foreach (var (fileName, fileContents) in files)
        {
            var options = new TSqlObjectOptions();
            try
            {
                model.AddOrUpdateObjects(fileContents, fileName, new TSqlObjectOptions());
            }
            catch (DacModelException dex)
            {
                result.ModelErrors.Add(fileName, dex);
            }
        }
    }

    private static void AddScriptToModel(AnalyzerResult result, TSqlModel model, string script)
    {
        var options = new TSqlObjectOptions();
        try
        {
            model.AddObjects(script, new TSqlObjectOptions());
        }
        catch (DacModelException dex)
        {
            result.ModelErrors.Add(Guid.NewGuid().ToString(), dex);
        }
    }

    private static TSqlModel CreateDacpacModel(string dacpacPath)
        => TSqlModel.LoadFromDacpac(
                dacpacPath,
                new ModelLoadOptions
                {
                    LoadAsScriptBackedModel = true,
                    ModelStorageType = DacSchemaModelStorageType.Memory,
                });

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

    private static FileInfo? GetOutputFile(FileInfo? fileInfo)
    {
        if (fileInfo == null)
        {
            return null;
        }

        if (!fileInfo.Extension.Equals(".xml", StringComparison.OrdinalIgnoreCase)
            && !fileInfo.Extension.Equals(".json", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Output file must be of type 'xml' or type 'json'");
        }

        if (!Path.IsPathRooted(fileInfo.FullName))
        {
            return new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), fileInfo.FullName));
        }

        return fileInfo;
    }
}
