using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.Rules.Tests.Helpers;

namespace SqlServer.Rules.Tests.Performance;

[TestClass]
public class SRP0028Tests : TestModel
{
    public SRP0028Tests()
        : base(TestConstants.SqlServerRules)
    {
    }

    [TestMethod]
    public void ExplicitRangeWindowDetected()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/ExplicitRangeWindowSrp0028Test.sql");

        ExpectedProblems.Add(new TestProblem(7, 27, "SqlServer.Rules.SRP0028"));

        RunTest();
    }

    [TestMethod]
    public void ExplicitRowsWindowIsClean()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/RowsWindowSrp0028CleanTest.sql");

        RunTest();
    }
}
