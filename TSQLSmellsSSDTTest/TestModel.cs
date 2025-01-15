using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TSQLSmellsSSDTTest;

public class TestModel
{
    public List<TestProblem> ExpectedProblems { get; set; } = new List<TestProblem>();
    public List<TestProblem> FoundProblems { get; set; } = new List<TestProblem>();
    public List<string> TestFiles { get; set; } = new List<String>();

    private TSqlModel Model { get; set; }

    public void BuildModel()
    {
        Model = new TSqlModel(SqlServerVersion.Sql110, null);
        AddFilesToModel();
    }

    public void AddFilesToModel()
    {
        foreach (string FileName in TestFiles)
        {
            String FileContent = string.Empty;
            using (var reader = new StreamReader(FileName))
            {
                FileContent += reader.ReadToEnd();
            }

            Model.AddObjects(FileContent);
        }
    }

    public void SerializeResultOutput(CodeAnalysisResult result)
    {
        foreach (SqlRuleProblem Problem in result.Problems)
        {
            // Only concern ourselves with our problems
            if (Problem.RuleId.StartsWith("Smells."))
            {
                TestProblem TestProblem = new TestProblem(Problem.StartLine, Problem.StartColumn, Problem.RuleId);
                FoundProblems.Add(TestProblem);
            }
        }
    }

    public void RunSCARules()
    {
        CodeAnalysisService service = new CodeAnalysisServiceFactory().CreateAnalysisService(Model.Version);
        CodeAnalysisResult result = service.Analyze(Model);
        SerializeResultOutput(result);

        CollectionAssert.AreEquivalent(FoundProblems, ExpectedProblems);
    }

    public void RunTest()
    {
        BuildModel();
        RunSCARules();
    }
}
