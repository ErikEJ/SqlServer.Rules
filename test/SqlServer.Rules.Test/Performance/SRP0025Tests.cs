using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace SqlServer.Rules.Tests.Performance;

[TestClass]
public class SRP0025Tests : TestModel
{
    public SRP0025Tests()
        : base(TestConstants.SqlServerRules)
    {
    }

    [TestMethod]
    public void SelectStarInExistsDetected()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/SelectStarInExistsTest.sql");

        ExpectedProblems.Add(new TestProblem(5, 15, "SqlServer.Rules.SRP0025"));

        RunTest();
    }

    [TestMethod]
    public void SelectOneInExistsClean()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/SelectStarInExistsCleanTest.sql");

        RunTest();
    }
}
