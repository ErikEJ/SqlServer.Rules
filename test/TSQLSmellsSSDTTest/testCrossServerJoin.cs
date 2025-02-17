using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestCrossServerJoin : TestModel
{
    public TestCrossServerJoin()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/TestCrossServerJoin.sql");

        ExpectedProblems.Add(new TestProblem(5, 18, "Smells.SML001"));
    }

    [TestMethod]
    public void CrossServerJoin()
    {
        RunTest();
    }
}

