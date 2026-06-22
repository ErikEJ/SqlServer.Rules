using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.Rules.Tests.Helpers;

namespace SqlServer.Rules.Tests.Design;

[TestClass]
public class SRD0004Tests : TestModel
{
    public SRD0004Tests()
        : base(TestConstants.SqlServerRules)
    {
    }

    [TestMethod]
    public void FKWithUnresolvedTableDoesNotThrow()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/FKWithUnresolvedTable.sql");

        // Regression test for SRD0004: referenced table may be absent from the model, and SRD0004 must not throw.
        ExpectedProblems.Add(new TestProblem(1, 1, "SqlServer.Rules.SRN0007"));
        ExpectedProblems.Add(new TestProblem(5, 5, "SqlServer.Rules.SRN0007"));

        RunTest();
    }
}
