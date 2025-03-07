using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace SqlServer.Rules.Tests.Design;

[TestClass]
public class SRD0067Tests : TestModel
{
    public SRD0067Tests()
        : base(TestConstants.SqlServerRules)
    {
    }

    [TestMethod]
    public void Capitalize()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/KeywordCapitalize.sql");

        ExpectedProblems.Add(new TestProblem(2, 5, "SqlServer.Rules.SRD0047"));
        ExpectedProblems.Add(new TestProblem(2, 5, "SqlServer.Rules.SRD0047"));
        ExpectedProblems.Add(new TestProblem(2, 5, "SqlServer.Rules.SRD0047"));
        ExpectedProblems.Add(new TestProblem(17, 17, "SqlServer.Rules.SRD0039"));
        ExpectedProblems.Add(new TestProblem(15, 1, "SqlServer.Rules.SRD0067"));
        ExpectedProblems.Add(new TestProblem(15, 1, "SqlServer.Rules.SRN0006"));
        ExpectedProblems.Add(new TestProblem(15, 1, "SqlServer.Rules.SRP0005"));
        ExpectedProblems.Add(new TestProblem(19, 3, "SqlServer.Rules.SRP0006"));
        ExpectedProblems.Add(new TestProblem(1, 1, "SqlServer.Rules.SRD0002"));
        ExpectedProblems.Add(new TestProblem(1, 1, "SqlServer.Rules.SRD0067"));
        ExpectedProblems.Add(new TestProblem(1, 1, "SqlServer.Rules.SRD0067"));
        ExpectedProblems.Add(new TestProblem(1, 1, "SqlServer.Rules.SRD0067"));
        ExpectedProblems.Add(new TestProblem(1, 1, "SqlServer.Rules.SRD0067"));
        ExpectedProblems.Add(new TestProblem(1, 1, "SqlServer.Rules.SRN0006"));
        ExpectedProblems.Add(new TestProblem(1, 1, "SqlServer.Rules.SRP0020"));
        ExpectedProblems.Add(new TestProblem(9, 1, "SqlServer.Rules.SRD0002"));
        ExpectedProblems.Add(new TestProblem(9, 1, "SqlServer.Rules.SRD0067"));
        ExpectedProblems.Add(new TestProblem(9, 1, "SqlServer.Rules.SRN0006"));
        ExpectedProblems.Add(new TestProblem(9, 1, "SqlServer.Rules.SRP0020"));
        ExpectedProblems.Add(new TestProblem(22, 1, "SqlServer.Rules.SRD0067"));
        ExpectedProblems.Add(new TestProblem(22, 1, "SqlServer.Rules.SRD0067"));
        ExpectedProblems.Add(new TestProblem(22, 1, "SqlServer.Rules.SRD0067"));
        ExpectedProblems.Add(new TestProblem(27, 5, "SqlServer.Rules.SRD0003"));
        ExpectedProblems.Add(new TestProblem(27, 5, "SqlServer.Rules.SRN0007"));

        RunTest();
    }
}
