using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace SqlServer.Rules.Tests.Design;

[TestClass]
public class SRD0077Tests : TestModel
{
    public SRD0077Tests()
        : base(TestConstants.SqlServerRules)
    {
    }

    [TestMethod]
    public void FetchMismatchDetected()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/FetchMismatchTest.sql");

        ExpectedProblems.Add(new TestProblem(8, 10, "SqlServer.Rules.SRD0033"));
        ExpectedProblems.Add(new TestProblem(9, 5, "SqlServer.Rules.SRD0077"));

        RunTest();
    }

    [TestMethod]
    public void FetchMatchClean()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/FetchMatchTest.sql");

        ExpectedProblems.Add(new TestProblem(8, 10, "SqlServer.Rules.SRD0033"));

        RunTest();
    }
}
