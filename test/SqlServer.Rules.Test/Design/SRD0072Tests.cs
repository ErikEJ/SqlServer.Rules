using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.Rules.Tests.Helpers;

namespace SqlServer.Rules.Tests.Design;

[TestClass]
public class SRD0072Tests : TestModel
{
    public SRD0072Tests()
        : base(TestConstants.SqlServerRules)
    {
    }

    [TestMethod]
    public void VariableSelfAssignDetected()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/VariableSelfAssignTest.sql");

        ExpectedProblems.Add(new TestProblem(5, 13, "SqlServer.Rules.SRD0079"));
        ExpectedProblems.Add(new TestProblem(6, 5, "SqlServer.Rules.SRD0072"));

        RunTest();
    }
}
