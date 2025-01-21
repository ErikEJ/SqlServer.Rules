using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class testCrossServerJoin : TestModel
{
    public testCrossServerJoin()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/TestCrossServerJoin.sql");

        ExpectedProblems.Add(new TestProblem(5, 18, "Smells.SML001"));
    }

    [TestMethod]
    public void CrossServerJoin()
    {
        RunTest();
    }
}

