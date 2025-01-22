using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSQLSmellsSSDTTest.TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class testSelectStarInCteTest : TestModel
{
    public testSelectStarInCteTest()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/TestSelectStarInCteTest.sql");

        ExpectedProblems.Add(new TestProblem(7, 8, "Smells.SML005"));
    }

    [TestMethod]
    public void SelectStarInCteTest()
    {
        RunTest();
    }
}

