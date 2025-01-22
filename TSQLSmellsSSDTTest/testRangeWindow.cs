using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSQLSmellsSSDTTest.TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestRangeWindow : TestModel
{
    public TestRangeWindow()
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

