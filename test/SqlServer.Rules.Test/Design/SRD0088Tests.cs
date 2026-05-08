using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace SqlServer.Rules.Tests.Design;

[TestClass]
public class SRD0088Tests : TestModel
{
    public SRD0088Tests()
        : base(TestConstants.SqlServerRules)
    {
    }

    [TestMethod]
    public void NumericRoundAbortOnDetected()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/NumericRoundAbortOn.sql");

        ExpectedProblems.Add(new TestProblem(3, 1, "SqlServer.Rules.SRD0088"));

        RunTest();
    }

    [TestMethod]
    public void NumericRoundAbortOnNotPresent()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/SETs2.sql");

        RunTest();
    }
}
