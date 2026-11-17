using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.Rules.Tests.Helpers;

namespace SqlServer.Rules.Tests.Design;

[TestClass]
public class SRD0097Tests : TestModel
{
    public SRD0097Tests()
        : base(TestConstants.SqlServerRules)
    {
    }

    [TestMethod]
    public void FlagsClassifiedUnprotectedColumnsOnTablesAndViews()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/SRD0097SensitiveColumns.sql");

        ExpectedProblems.Add(new TestProblem(9, 5, "SqlServer.Rules.SRD0097"));
        ExpectedProblems.Add(new TestProblem(9, 5, "SqlServer.Rules.SRD0097"));

        RunTest();
    }
}
