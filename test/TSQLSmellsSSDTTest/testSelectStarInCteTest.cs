using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestSelectStarInCteTest : TestModel
{
    public TestSelectStarInCteTest()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/TestSelectStarInCteTest.sql");

        ExpectedProblems.Add(new TestProblem(7, 8, "Smells.SML005"));
    }

    [TestMethod]
    public void SelectStarInCteTest()
    {
        RunTest();
    }
}

