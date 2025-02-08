using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestSets : TestModel
{
    public TestSets()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/SETs.sql");

        ExpectedProblems.Add(new TestProblem(10, 1, "Smells.SML013"));
        ExpectedProblems.Add(new TestProblem(4, 1, "Smells.SML014"));
        ExpectedProblems.Add(new TestProblem(5, 1, "Smells.SML015"));
        ExpectedProblems.Add(new TestProblem(6, 1, "Smells.SML016"));
        ExpectedProblems.Add(new TestProblem(7, 1, "Smells.SML017"));
        ExpectedProblems.Add(new TestProblem(8, 1, "Smells.SML018"));
        ExpectedProblems.Add(new TestProblem(9, 1, "Smells.SML019"));
        ExpectedProblems.Add(new TestProblem(2, 18, "Smells.SML030"));
    }

    [TestMethod]
    public void Sets()
    {
        RunTest();
    }
}

