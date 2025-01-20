using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class testRangeWindow : TestModel
{
    public testRangeWindow()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/RangeWindow.sql");

        ExpectedProblems.Add(new TestProblem(8, 19, "Smells.SML025"));
    }

    [TestMethod]
    public void RangeWindow()
    {
        RunTest();
    }
}
#pragma warning restore IDE1006 // Naming Styles
