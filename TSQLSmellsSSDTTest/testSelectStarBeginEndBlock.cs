using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class testSelectStarBeginEndBlock : TestModel
{
    public testSelectStarBeginEndBlock()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/TestSelectStarBeginEndBlock.sql");

        ExpectedProblems.Add(new TestProblem(6, 9, "Smells.SML005"));
    }

    [TestMethod]
    public void SelectStarBeginEndBlock()
    {
        RunTest();
    }
}

