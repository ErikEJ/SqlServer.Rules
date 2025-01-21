using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class testOrderByOrdinal : TestModel
{
    public testOrderByOrdinal()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/orderbyordinal.sql");

        ExpectedProblems.Add(new TestProblem(6, 34, "Smells.SML007"));
        ExpectedProblems.Add(new TestProblem(6, 36, "Smells.SML007"));
        ExpectedProblems.Add(new TestProblem(6, 38, "Smells.SML007"));
    }

    [TestMethod]
    public void OrderByOrdinal()
    {
        RunTest();
    }
}

