using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace SqlServer.Rules.Tests.Design;

[TestClass]
public class SRD0090Tests : TestModel
{
    public SRD0090Tests()
        : base(TestConstants.SqlServerRules)
    {
    }

    [TestMethod]
    public void ForcePlanOnDetected()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/ForcePlanOnTest.sql");

        ExpectedProblems.Add(new TestProblem(5, 5, "SqlServer.Rules.SRD0090"));

        RunTest();
    }
}
