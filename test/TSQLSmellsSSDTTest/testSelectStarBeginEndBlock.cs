using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestSelectStarBeginEndBlock : TestModel
{
    public TestSelectStarBeginEndBlock()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/TestSelectStarBeginEndBlock.sql");

        ExpectedProblems.Add(new TestProblem(6, 9, "Smells.SML005"));
    }

    [TestMethod]
    public void SelectStarBeginEndBlock()
    {
        RunTest();
    }
}

