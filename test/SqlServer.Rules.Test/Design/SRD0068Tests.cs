using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace SqlServer.Rules.Tests.Design;

[TestClass]
public class SRD0068Tests : TestModel
{
    public SRD0068Tests()
        : base(TestConstants.SqlServerRules)
    {
    }

    [TestMethod]
    public void RequireSemicolon()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/MissingSemicolons.sql");

        ExpectedProblems.Add(new TestProblem(40, 1, "SqlServer.Rules.SRD0068"));
        ExpectedProblems.Add(new TestProblem(50, 22, "SqlServer.Rules.SRD0039"));
        ExpectedProblems.Add(new TestProblem(43, 1, "SqlServer.Rules.SRD0068"));
        ExpectedProblems.Add(new TestProblem(7, 17, "SqlServer.Rules.SRD0039"));
        ExpectedProblems.Add(new TestProblem(16, 1, "SqlServer.Rules.SRD0057"));
        ExpectedProblems.Add(new TestProblem(1, 1, "SqlServer.Rules.SRD0067"));
        ExpectedProblems.Add(new TestProblem(1, 1, "SqlServer.Rules.SRD0068"));
        ExpectedProblems.Add(new TestProblem(5, 1, "SqlServer.Rules.SRD0068"));
        ExpectedProblems.Add(new TestProblem(7, 1, "SqlServer.Rules.SRD0068"));
        ExpectedProblems.Add(new TestProblem(9, 1, "SqlServer.Rules.SRD0068"));
        ExpectedProblems.Add(new TestProblem(10, 5, "SqlServer.Rules.SRD0068"));
        ExpectedProblems.Add(new TestProblem(16, 1, "SqlServer.Rules.SRD0068"));
        ExpectedProblems.Add(new TestProblem(20, 1, "SqlServer.Rules.SRD0068"));
        ExpectedProblems.Add(new TestProblem(24, 5, "SqlServer.Rules.SRD0068"));
        ExpectedProblems.Add(new TestProblem(34, 5, "SqlServer.Rules.SRD0068"));
        ExpectedProblems.Add(new TestProblem(22, 14, "SqlServer.Rules.SRP0025"));
        ExpectedProblems.Add(new TestProblem(28, 14, "SqlServer.Rules.SRP0025"));

        RunTest();
    }

    [TestMethod]
    public void HasSemicolon()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/NoMissingSemicolons.sql");

        ExpectedProblems.Add(new TestProblem(50, 22, "SqlServer.Rules.SRD0039"));
        ExpectedProblems.Add(new TestProblem(7, 17, "SqlServer.Rules.SRD0039"));
        ExpectedProblems.Add(new TestProblem(16, 1, "SqlServer.Rules.SRD0057"));
        ExpectedProblems.Add(new TestProblem(1, 1, "SqlServer.Rules.SRD0067"));
        ExpectedProblems.Add(new TestProblem(22, 14, "SqlServer.Rules.SRP0025"));
        ExpectedProblems.Add(new TestProblem(28, 14, "SqlServer.Rules.SRP0025"));

        RunTest();
    }
}
