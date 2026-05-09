using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace SqlServer.Rules.Tests.Design;

[TestClass]
public class SRD0085Tests : TestModel
{
    public SRD0085Tests()
        : base(TestConstants.SqlServerRules)
    {
    }

    [TestMethod]
    public void AnsiNullsOffDetected()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/AnsiNullsOffTest.sql");

        ExpectedProblems.Add(new TestProblem(5, 5, "SqlServer.Rules.SRD0085"));

        RunTest();
    }

    [TestMethod]
    public void AnsiNullsOnIgnored()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/AnsiNullsOnCleanTest.sql");

        RunTest();
    }
}
