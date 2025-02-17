using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestSelectStarFromViewInProc : TestModel
{
    public TestSelectStarFromViewInProc()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/SelectStarFromViewInProc.sql");

        ExpectedProblems.Add(new TestProblem(4, 8, "Smells.SML005"));
    }

    [TestMethod]
    public void SelectFromTableVar()
    {
        RunTest();
    }
}

