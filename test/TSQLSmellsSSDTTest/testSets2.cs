using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestSets2 : TestModel
{
    public TestSets2()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/SETs2.sql");

        ExpectedProblems.Add(new TestProblem(5, 16, "Smells.SML008"));
        ExpectedProblems.Add(new TestProblem(6, 15, "Smells.SML009"));
        ExpectedProblems.Add(new TestProblem(7, 1, "Smells.SML020"));
        ExpectedProblems.Add(new TestProblem(8, 1, "Smells.SML022"));
    }

    [TestMethod]
    public void Sets2()
    {
        RunTest();
    }
}

