using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace SqlServer.Rules.Tests.Design;

[TestClass]
public class SRD0073Tests : TestModel
{
    public SRD0073Tests()
        : base(TestConstants.SqlServerRules)
    {
    }

    [TestMethod]
    public void RepeatedNegationDetected()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/RepeatedNotTest.sql");

        ExpectedProblems.Add(new TestProblem(6, 8, "SqlServer.Rules.SRD0073"));

        RunTest();
    }
}
