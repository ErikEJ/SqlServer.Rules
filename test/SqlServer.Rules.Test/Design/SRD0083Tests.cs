using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.Rules.Tests.Helpers;

namespace SqlServer.Rules.Tests.Design;

[TestClass]
public class SRD0083Tests : TestModel
{
    public SRD0083Tests()
        : base(TestConstants.SqlServerRules)
    {
    }

    [TestMethod]
    public void ChangeDateFirstDetected()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/SRD0083_SetDateFirst.sql");

        ExpectedProblems.Add(new TestProblem(5, 5, "SqlServer.Rules.SRD0083"));

        RunTest();
    }
}
