using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class testSelectStarFromViewInProc : TestModel
{
    public testSelectStarFromViewInProc()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/SelectStarFromViewInProc.sql");

        ExpectedProblems.Add(new TestProblem(4, 8, "Smells.SML005"));
    }

    [TestMethod]
    public void SelectFromTableVar()
    {
        RunTest();
    }
}
#pragma warning restore IDE1006 // Naming Styles
