using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace SqlServer.Rules.Tests.SmokeTests;

[TestClass]
public class TestFabric : TestModel
{
    public TestFabric()
        : base(TestConstants.SqlServerRules)
    {
    }

    [TestMethod]
    public void TestFabricDW()
    {
        foreach (var fileName in Directory.GetFiles("../../../../../sqlprojects/ForsDW", "*.sql"))
        {
            TestFiles.Add(fileName);
        }

        ExpectedProblems.Add(new TestProblem(3, 15, "SqlServer.Rules.SRD0006"));
        ExpectedProblems.Add(new TestProblem(3, 8, "SqlServer.Rules.SRD0014"));
        ExpectedProblems.Add(new TestProblem(3, 1, "SqlServer.Rules.SRD0068"));
        ExpectedProblems.Add(new TestProblem(2, 1, "SqlServer.Rules.SRN0006"));
        ExpectedProblems.Add(new TestProblem(2, 15, "SqlServer.Rules.SRD0006"));
        ExpectedProblems.Add(new TestProblem(2, 8, "SqlServer.Rules.SRD0014"));
        ExpectedProblems.Add(new TestProblem(1, 1, "SqlServer.Rules.SRD0068"));
        ExpectedProblems.Add(new TestProblem(1, 1, "SqlServer.Rules.SRN0006"));
        ExpectedProblems.Add(new TestProblem(3, 15, "SqlServer.Rules.SRD0006"));
        ExpectedProblems.Add(new TestProblem(3, 8, "SqlServer.Rules.SRD0014"));
        ExpectedProblems.Add(new TestProblem(1, 1, "SqlServer.Rules.SRD0068"));
        ExpectedProblems.Add(new TestProblem(3, 1, "SqlServer.Rules.SRD0068"));
        ExpectedProblems.Add(new TestProblem(1, 1, "SqlServer.Rules.SRN0006"));
        ExpectedProblems.Add(new TestProblem(1, 1, "SqlServer.Rules.SRP0005"));

        RunTest();
    }
}

