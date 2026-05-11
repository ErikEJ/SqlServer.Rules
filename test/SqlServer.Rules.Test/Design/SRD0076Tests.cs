using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.Rules.Tests.Helpers;

namespace SqlServer.Rules.Tests.Design;

[TestClass]
public class SRD0076Tests : TestModel
{
    public SRD0076Tests()
        : base(TestConstants.SqlServerRules)
    {
    }

    [TestMethod]
    public void IdenticalExpressionsDetected()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/IdenticalExprTest.sql");

        ExpectedProblems.Add(new TestProblem(5, 13, "SqlServer.Rules.SRD0079"));
        ExpectedProblems.Add(new TestProblem(6, 8, "SqlServer.Rules.SRD0076"));

        RunTest();
    }
}
