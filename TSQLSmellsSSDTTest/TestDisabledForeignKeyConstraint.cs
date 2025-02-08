using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestDisabledForeignKeyConstraint : TestModel
{
    public TestDisabledForeignKeyConstraint()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/DisabledForeignKey.sql");

        ExpectedProblems.Add(new TestProblem(7, 7, "Smells.SML006"));
        ExpectedProblems.Add(new TestProblem(8, 5, "Smells.SML006"));
    }

    [TestMethod]
    [Ignore("Not working")]
    public void DisabledForeignKeyConstraint()
    {
        RunTest();
    }
}
