using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace SqlServer.Rules.Tests.Design;

[TestClass]
public class SRD0066Tests : TestModel
{
    public SRD0066Tests()
        : base(TestConstants.SqlServerRules)
    {
    }

    [TestMethod]
    public void UseBeginEnd()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/BeginEnd.sql");

        ExpectedProblems.Add(new TestProblem(7, 3, "SqlServer.Rules.SRD0066"));
        ExpectedProblems.Add(new TestProblem(9, 3, "SqlServer.Rules.SRD0066"));

        RunTest();
    }
}
