using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace SqlServer.Rules.Tests.SmokeTests;

[TestClass]
public class TestChinook : TestModel
{
    public TestChinook()
    : base(TestConstants.SqlServerRules)
    {
    }

    [TestMethod]
    public void TestChinookDatabase()
    {
        foreach (var fileName in Directory.GetFiles("../../../../../sqlprojects/Chinook/Tables", "*.sql"))
        {
            TestFiles.Add(fileName);
        }

        ExpectedProblems.Add(new TestProblem(3, 5, "SqlServer.Rules.SRD0047"));
        ExpectedProblems.Add(new TestProblem(3, 5, "SqlServer.Rules.SRD0047"));
        ExpectedProblems.Add(new TestProblem(3, 5, "SqlServer.Rules.SRD0047"));
        ExpectedProblems.Add(new TestProblem(4, 5, "SqlServer.Rules.SRD0047"));
        ExpectedProblems.Add(new TestProblem(5, 5, "SqlServer.Rules.SRD0047"));
        ExpectedProblems.Add(new TestProblem(3, 5, "SqlServer.Rules.SRD0047"));
        ExpectedProblems.Add(new TestProblem(3, 5, "SqlServer.Rules.SRD0047"));
        ExpectedProblems.Add(new TestProblem(3, 5, "SqlServer.Rules.SRD0047"));
        ExpectedProblems.Add(new TestProblem(3, 5, "SqlServer.Rules.SRD0047"));
        ExpectedProblems.Add(new TestProblem(4, 10, "SqlServer.Rules.SRD0038"));
        ExpectedProblems.Add(new TestProblem(5, 19, "SqlServer.Rules.SRD0038"));
        ExpectedProblems.Add(new TestProblem(1, 1, "SqlServer.Rules.SRD0001"));
        ExpectedProblems.Add(new TestProblem(2, 5, "SqlServer.Rules.SRD0010"));
        ExpectedProblems.Add(new TestProblem(1, 1, "SqlServer.Rules.SRD0001"));
        ExpectedProblems.Add(new TestProblem(2, 5, "SqlServer.Rules.SRD0010"));
        ExpectedProblems.Add(new TestProblem(1, 1, "SqlServer.Rules.SRD0001"));
        ExpectedProblems.Add(new TestProblem(2, 5, "SqlServer.Rules.SRD0010"));
        ExpectedProblems.Add(new TestProblem(1, 1, "SqlServer.Rules.SRD0001"));
        ExpectedProblems.Add(new TestProblem(2, 5, "SqlServer.Rules.SRD0010"));
        ExpectedProblems.Add(new TestProblem(1, 1, "SqlServer.Rules.SRD0001"));
        ExpectedProblems.Add(new TestProblem(2, 5, "SqlServer.Rules.SRD0010"));
        ExpectedProblems.Add(new TestProblem(1, 1, "SqlServer.Rules.SRD0001"));
        ExpectedProblems.Add(new TestProblem(2, 5, "SqlServer.Rules.SRD0010"));
        ExpectedProblems.Add(new TestProblem(1, 1, "SqlServer.Rules.SRD0001"));
        ExpectedProblems.Add(new TestProblem(2, 5, "SqlServer.Rules.SRD0010"));
        ExpectedProblems.Add(new TestProblem(1, 1, "SqlServer.Rules.SRD0001"));
        ExpectedProblems.Add(new TestProblem(2, 5, "SqlServer.Rules.SRD0010"));
        ExpectedProblems.Add(new TestProblem(1, 1, "SqlServer.Rules.SRD0001"));
        ExpectedProblems.Add(new TestProblem(2, 5, "SqlServer.Rules.SRD0010"));
        ExpectedProblems.Add(new TestProblem(1, 1, "SqlServer.Rules.SRP0020"));
        ExpectedProblems.Add(new TestProblem(1, 1, "SqlServer.Rules.SRD0001"));
        ExpectedProblems.Add(new TestProblem(2, 5, "SqlServer.Rules.SRD0010"));
        ExpectedProblems.Add(new TestProblem(12, 1, "SqlServer.Rules.SRN0007"));
        ExpectedProblems.Add(new TestProblem(22, 1, "SqlServer.Rules.SRN0007"));
        ExpectedProblems.Add(new TestProblem(24, 1, "SqlServer.Rules.SRN0007"));
        ExpectedProblems.Add(new TestProblem(18, 1, "SqlServer.Rules.SRN0007"));
        ExpectedProblems.Add(new TestProblem(15, 1, "SqlServer.Rules.SRN0007"));
        ExpectedProblems.Add(new TestProblem(21, 1, "SqlServer.Rules.SRN0007"));
        ExpectedProblems.Add(new TestProblem(12, 1, "SqlServer.Rules.SRN0007"));
        ExpectedProblems.Add(new TestProblem(20, 1, "SqlServer.Rules.SRN0007"));
        ExpectedProblems.Add(new TestProblem(26, 1, "SqlServer.Rules.SRN0007"));
        ExpectedProblems.Add(new TestProblem(32, 1, "SqlServer.Rules.SRN0007"));
        ExpectedProblems.Add(new TestProblem(6, 5, "SqlServer.Rules.SRN0007"));
        ExpectedProblems.Add(new TestProblem(16, 5, "SqlServer.Rules.SRN0007"));
        ExpectedProblems.Add(new TestProblem(18, 5, "SqlServer.Rules.SRN0007"));
        ExpectedProblems.Add(new TestProblem(12, 5, "SqlServer.Rules.SRN0007"));
        ExpectedProblems.Add(new TestProblem(8, 5, "SqlServer.Rules.SRN0007"));
        ExpectedProblems.Add(new TestProblem(9, 5, "SqlServer.Rules.SRN0007"));
        ExpectedProblems.Add(new TestProblem(5, 5, "SqlServer.Rules.SRN0007"));
        ExpectedProblems.Add(new TestProblem(6, 5, "SqlServer.Rules.SRN0007"));
        ExpectedProblems.Add(new TestProblem(12, 5, "SqlServer.Rules.SRN0007"));
        ExpectedProblems.Add(new TestProblem(13, 5, "SqlServer.Rules.SRN0007"));
        ExpectedProblems.Add(new TestProblem(14, 5, "SqlServer.Rules.SRN0007"));

        RunTest();
    }
}
