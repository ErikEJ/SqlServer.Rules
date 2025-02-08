using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestSelectStarInWhileLoop : TestModel
{
    public TestSelectStarInWhileLoop()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/TestSelectStarInWhileLoop.sql");

        ExpectedProblems.Add(new TestProblem(5, 9, "Smells.SML005"));
    }

    [TestMethod]
    public void SelectStarInWhileLoop()
    {
        RunTest();
    }
}

