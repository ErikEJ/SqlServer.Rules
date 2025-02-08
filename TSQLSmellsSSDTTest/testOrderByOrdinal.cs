using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestOrderByOrdinal : TestModel
{
    public TestOrderByOrdinal()
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

