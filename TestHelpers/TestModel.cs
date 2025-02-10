using System.Collections.Generic;
using System.IO;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestHelpers;

public class TestModel
{
    private TSqlModel Model { get; set; }

    private string Prefix { get; set; }

    public TestModel(string prefix = "Smells.")
    {
        Model = new TSqlModel(SqlServerVersion.Sql150, null);
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

            Model.AddObjects(fileContent);
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

        CollectionAssert.AreEquivalent(ExpectedProblems, FoundProblems);
    }

    public void RunTest()
    {
        AddFilesToModel();
        RunSCARules();
    }
}
