using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class testExplicitRangeWindow : TestModel
{
    public testExplicitRangeWindow()
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

