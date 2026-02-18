using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace SqlServer.Rules.Tests.Design;

[TestClass]
public class SRD0070Tests : TestModel
{
    public SRD0070Tests()
        : base(TestConstants.SqlServerRules)
    {
    }

    [TestMethod]
    public void GotoDetected()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/GotoTest.sql");

        ExpectedProblems.Add(new TestProblem(9, 9, "SqlServer.Rules.SRD0066"));
        ExpectedProblems.Add(new TestProblem(9, 9, "SqlServer.Rules.SRD0070"));

        RunTest();
    }

    [TestMethod]
    public void NoGotoClean()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/GotoTestClean.sql");

        RunTest();
    }
}
