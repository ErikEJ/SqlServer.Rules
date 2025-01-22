using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSQLSmellsSSDTTest.TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class testSelectStarIninlineTVF : TestModel
{
    public testSelectStarIninlineTVF()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/TestSelectStarIninlineTVF.sql");

        ExpectedProblems.Add(new TestProblem(6, 9, "Smells.SML005"));
    }

    [TestMethod]
    public void SelectStarIninlineTVF()
    {
        RunTest();
    }
}

