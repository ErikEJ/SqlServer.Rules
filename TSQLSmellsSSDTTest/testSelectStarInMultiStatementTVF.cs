using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class testSelectStarInMultiStatementTVF : TestModel
{
    public testSelectStarInMultiStatementTVF()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/TestSelectStarInMultiStatementTVF.sql");

        ExpectedProblems.Add(new TestProblem(12, 10, "Smells.SML005"));
        ExpectedProblems.Add(new TestProblem(8, 10, "Smells.SML033"));
    }

    [TestMethod]
    public void SelectStarInMultiStatementTVF()
    {
        RunTest();
    }
}

