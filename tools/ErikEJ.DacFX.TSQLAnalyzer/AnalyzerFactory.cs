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

    public AnalyzerFactory(AnalyzerOptions analyzerOptions)
    {
        request = analyzerOptions;
    }

    public AnalyzerResult Analyze()
    {
        var result = new AnalyzerResult();

        if (request.Scripts == null && request.ConnectionString == null)
        {
            throw new ArgumentException("No scripts or connection string specified");
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
            }

            result.Result = analysisResult;
        }

        return result;
    }

    private TSqlModel GenerateTSqlModel(AnalyzerResult result)
    {
        var model = new TSqlModel(request.SqlVersion, new TSqlModelOptions());

        if (request.Scripts != null && request.ConnectionString == null)
        {
            if (request.Scripts.Count == 0)
            {
                throw new ArgumentException("No files to analyze");
            }

            var files = sqlFileCollector.ProcessList(request.Scripts);

            result.FileCount = files.Count;

            if (files.Count == 0)
            {
                throw new ArgumentException("No files found to analyze");
            }

            if (files.Count == 1 && files.First().Key.EndsWith(".dacpac", StringComparison.OrdinalIgnoreCase))
            {
                model = TSqlModel.LoadFromDacpac(
                    files.First().Key,
                    new ModelLoadOptions
                    {
                        LoadAsScriptBackedModel = true,
                        ModelStorageType = Microsoft.SqlServer.Dac.DacSchemaModelStorageType.Memory,
                    });
            }
            else
            {
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
        }
        else if (request.ConnectionString != null)
        {
            var extractOptions = new ModelExtractOptions
            {
                VerifyExtraction = true,
                IgnorePermissions = true,
                IgnoreUserLoginMappings = true,
                IgnoreExtendedProperties = true,
                Storage = DacSchemaModelStorageType.Memory,
            };

            model = TSqlModel.LoadFromDatabase(request.ConnectionString.ConnectionString, extractOptions);
        }

        return model;
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

    private static FileInfo? GetOutputFile(FileInfo? fileInfo)
    {
        if (fileInfo == null)
        {
            return null;
        }

        if (!fileInfo.Extension.Equals(".xml", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Output file must be of type 'xml'");
        }

        if (!Path.IsPathRooted(fileInfo.FullName))
        {
            return new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), fileInfo.FullName));
        }

        return fileInfo;
    }
}