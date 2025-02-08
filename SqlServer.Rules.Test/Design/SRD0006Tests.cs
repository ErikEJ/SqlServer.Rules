using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace SqlServer.Rules.Tests.Design;

[TestClass]
public class SRD0006Tests : TestModel
{
    public SRD0006Tests()
        : base(TestConstants.SqlServerRules)
    {
    }

    [TestMethod]
    public void SelectStarBeginEndBlock()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/TestSelectStarBeginEndBlock.sql");

        ExpectedProblems.Add(new TestProblem(6, 9, "SqlServer.Rules.SRD0006"));

        RunTest();
    }
}
