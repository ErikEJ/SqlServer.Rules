using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace SqlServer.Rules.Tests.Design;

[TestClass]
public class SRD0002Tests : TestModel
{
    public SRD0002Tests()
        : base(TestConstants.SqlServerRules)
    {
    }

    [TestMethod]
    public void MustHavePrimaryKey()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/CreateTableNoSchema.sql");
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/CreateTableClusteredColumnStore.sql");
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/CreateTableTemporalHistory.sql");

        ExpectedProblems.Add(new TestProblem(1, 1, "SqlServer.Rules.SRD0002"));
        ExpectedProblems.Add(new TestProblem(1, 1, "SqlServer.Rules.SRD0067"));
        ExpectedProblems.Add(new TestProblem(1, 1, "SqlServer.Rules.SRD0067"));
        ExpectedProblems.Add(new TestProblem(1, 1, "SqlServer.Rules.SRN0006"));
        ExpectedProblems.Add(new TestProblem(1, 1, "SqlServer.Rules.SRP0020"));
        ExpectedProblems.Add(new TestProblem(1, 1, "SqlServer.Rules.SRD0001"));
        ExpectedProblems.Add(new TestProblem(23, 1, "SqlServer.Rules.SRN0007"));

        RunTest();
    }
}
