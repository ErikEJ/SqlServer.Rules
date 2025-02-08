using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestExplicitRangeWindow : TestModel
{
    public TestExplicitRangeWindow()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/ExplicitRangeWindow.sql");

        ExpectedProblems.Add(new TestProblem(7, 19, "Smells.SML025"));
    }

    [TestMethod]
    public void ExplicitRangeWindow()
    {
        RunTest();
    }
}

