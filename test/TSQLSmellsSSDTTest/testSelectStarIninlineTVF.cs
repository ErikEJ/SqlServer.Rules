using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestSelectStarIninlineTVF : TestModel
{
    public TestSelectStarIninlineTVF()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/TestSelectStarIninlineTVF.sql");

        ExpectedProblems.Add(new TestProblem(6, 9, "Smells.SML005"));
    }

    [TestMethod]
    public void SelectStarIninlineTVF()
    {
        RunTest();
    }
}

