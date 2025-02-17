using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestSelectStarInMultiStatementTVF : TestModel
{
    public TestSelectStarInMultiStatementTVF()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/TestSelectStarInMultiStatementTVF.sql");

        ExpectedProblems.Add(new TestProblem(12, 10, "Smells.SML005"));
        ExpectedProblems.Add(new TestProblem(8, 10, "Smells.SML033"));
    }

    [TestMethod]
    public void SelectStarInMultiStatementTVF()
    {
        RunTest();
    }
}

