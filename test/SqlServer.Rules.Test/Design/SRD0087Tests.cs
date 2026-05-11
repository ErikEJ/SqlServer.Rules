using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.Rules.Tests.Helpers;

namespace SqlServer.Rules.Tests.Design;

[TestClass]
public class SRD0087Tests : TestModel
{
    public SRD0087Tests()
        : base(TestConstants.SqlServerRules)
    {
    }

    [TestMethod]
    public void AnsiWarningsOffDetected()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/SRD0087_AnsiWarningsOn.sql");

        ExpectedProblems.Add(new TestProblem(1, 1, "SqlServer.Rules.SRP0005"));
        ExpectedProblems.Add(new TestProblem(3, 1, "SqlServer.Rules.SRD0068"));
        ExpectedProblems.Add(new TestProblem(3, 1, "SqlServer.Rules.SRD0087"));

        RunTest();
    }
}
