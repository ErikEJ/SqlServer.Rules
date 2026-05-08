using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace SqlServer.Rules.Tests.Design;

[TestClass]
public class SRD0081Tests : TestModel
{
    public SRD0081Tests()
        : base(TestConstants.SqlServerRules)
    {
    }

    [TestMethod]
    public void TopHundredPercentDetected()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/TopHundredPercentTest.sql");

        ExpectedProblems.Add(new TestProblem(5, 12, "SqlServer.Rules.SRD0081"));

        RunTest();
    }
}
