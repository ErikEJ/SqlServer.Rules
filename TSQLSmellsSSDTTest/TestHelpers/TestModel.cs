using System.Collections.Generic;
using System.IO;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TSQLSmellsSSDTTest.TestHelpers;

public class TestModel
{
    public List<TestProblem> ExpectedProblems { get; private set; } = [];

    public List<TestProblem> FoundProblems { get; private set; } = [];

    public List<string> TestFiles { get; private set; } = [];

    private TSqlModel Model { get; set; }

    public void BuildModel()
    {
        Model = new TSqlModel(SqlServerVersion.Sql110, null);
        AddFilesToModel();
    }

    public void AddFilesToModel()
    {
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
        foreach (var FileName in TestFiles)
        {
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
            var FileContent = string.Empty;
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
            using (var reader = new StreamReader(FileName))
            {
                FileContent += reader.ReadToEnd();
            }

            Model.AddObjects(FileContent);
        }
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
    }

    public void SerializeResultOutput(CodeAnalysisResult result)
    {
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
        foreach (var Problem in result.Problems)
        {
            // Only concern ourselves with our problems
            if (Problem.RuleId.StartsWith("Smells.", System.StringComparison.OrdinalIgnoreCase))
            {
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
                var TestProblem = new TestProblem(Problem.StartLine, Problem.StartColumn, Problem.RuleId);
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
                FoundProblems.Add(TestProblem);
            }
        }
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
    }

    public void RunSCARules()
    {
        var service = new CodeAnalysisServiceFactory().CreateAnalysisService(Model.Version);
        var result = service.Analyze(Model);
        SerializeResultOutput(result);

        CollectionAssert.AreEquivalent(FoundProblems, ExpectedProblems);
    }

    public void RunTest()
    {
        BuildModel();
        RunSCARules();
    }
}
