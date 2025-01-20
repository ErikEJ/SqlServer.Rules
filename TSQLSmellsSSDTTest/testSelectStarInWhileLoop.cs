using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class testSelectStarInWhileLoop : TestModel
{
    public testSelectStarInWhileLoop()
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
#pragma warning restore IDE1006 // Naming Styles
