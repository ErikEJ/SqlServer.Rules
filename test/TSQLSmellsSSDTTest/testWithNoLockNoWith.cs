using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestWithNoLockNoWith : TestModel
{
    public TestWithNoLockNoWith()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/TestWithNoLockNoWith.sql");

        ExpectedProblems.Add(new TestProblem(4, 38, "Smells.SML003"));
    }

    [TestMethod]
    public void WithNoLockNoWith()
    {
        RunTest();
    }
}

