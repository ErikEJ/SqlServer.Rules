using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlServer.Rules.Tests.Helpers;

public class TestModel
{
    private TSqlModel Model { get; set; }

    private string Prefix { get; set; }

    public TestModel(string prefix = TestConstants.SqlServerRules)
    {
        Model = new TSqlModel(SqlServerVersion.Sql150, new TSqlModelOptions { PageVerifyMode = PageVerifyMode.Checksum });
        Prefix = prefix;
    }

    public List<TestProblem> ExpectedProblems { get; private set; } = [];

    public List<TestProblem> FoundProblems { get; private set; } = [];

    public List<string> TestFiles { get; private set; } = [];

    public void AddFilesToModel()
    {
        foreach (var fileName in TestFiles)
        {
            var fileContent = string.Empty;
            using (var reader = new StreamReader(fileName))
            {
                fileContent += reader.ReadToEnd();
            }

            Model.AddOrUpdateObjects(fileContent, fileName, new TSqlObjectOptions());
        }
    }

    public void SerializeResultOutput(CodeAnalysisResult result)
    {
        foreach (var problem in result.Problems)
        {
            // Only concern ourselves with our problems
            if (problem.RuleId.StartsWith(Prefix, System.StringComparison.OrdinalIgnoreCase))
            {
                var testProblem = new TestProblem(problem.StartLine, problem.StartColumn, problem.RuleId);

                FoundProblems.Add(testProblem);
            }
        }
    }

    public void RunSCARules()
    {
        var service = new CodeAnalysisServiceFactory().CreateAnalysisService(Model.Version);
        var result = service.Analyze(Model);
        SerializeResultOutput(result);

        var problemsBuilder = new StringBuilder();

        problemsBuilder.AppendLine();

        foreach (var problem in result.Problems)
        {
            problemsBuilder.AppendLine(CultureInfo.InvariantCulture, $"{problem.StartLine}, {problem.StartColumn}, {problem.RuleId}: {problem.ShortErrorMessage}, ");
        }

        CollectionAssert.AreEquivalent(ExpectedProblems, FoundProblems, problemsBuilder.ToString());
    }

    public void RunTest()
    {
        AddFilesToModel();
        RunSCARules();
    }
}
