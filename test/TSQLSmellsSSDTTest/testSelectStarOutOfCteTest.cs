using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestSelectStarOutOfCteTest : TestModel
{
    public TestSelectStarOutOfCteTest()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/SelectStarOutOfCteTest.sql");

        ExpectedProblems.Add(new TestProblem(8, 8, "Smells.SML005"));
        ExpectedProblems.Add(new TestProblem(10, 8, "Smells.SML005"));
    }

    [TestMethod]
    public void SelectStarOutOfCteTest()
    {
        RunTest();
    }
}

