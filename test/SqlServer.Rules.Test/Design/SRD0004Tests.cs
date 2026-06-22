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

        // Rule fires for the source table side (FK column unindexed), but must not throw when the
        // referenced table is absent from the model (regression test for NRE bug).
        ExpectedProblems.Add(new TestProblem(1, 1, "SqlServer.Rules.SRN0007"));
        ExpectedProblems.Add(new TestProblem(5, 5, "SqlServer.Rules.SRD0004"));
        ExpectedProblems.Add(new TestProblem(5, 5, "SqlServer.Rules.SRN0007"));

        RunTest();
    }
}
