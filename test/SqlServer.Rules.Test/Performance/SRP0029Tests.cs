using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.Rules.Tests.Helpers;

namespace SqlServer.Rules.Tests.Performance;

[TestClass]
public class SRP0029Tests : TestModel
{
    public SRP0029Tests()
        : base(TestConstants.SqlServerRules)
    {
    }

    [TestMethod]
    public void ImplicitRangeWindowDetected()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/ImplicitRangeWindowSrp0029Test.sql");

        ExpectedProblems.Add(new TestProblem(6, 21, "SqlServer.Rules.SRP0029"));

        RunTest();
    }

    [TestMethod]
    public void RankingFunctionWithoutFrameIsIgnored()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/ImplicitRangeWindowSrp0029IgnoredFunctionsTest.sql");

        RunTest();
    }
}
