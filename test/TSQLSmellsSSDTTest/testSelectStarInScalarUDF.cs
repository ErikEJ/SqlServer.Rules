using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestSelectStarInScalarUDF : TestModel
{
    public TestSelectStarInScalarUDF()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/TestSelectStarInScalarUDF.sql");

        ExpectedProblems.Add(new TestProblem(9, 10, "Smells.SML005"));
        ExpectedProblems.Add(new TestProblem(5, 10, "Smells.SML033"));
    }

    [TestMethod]
    public void SelectStarInScalarUDF()
    {
        RunTest();
    }
}

