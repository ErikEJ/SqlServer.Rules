using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSQLSmellsSSDTTest.TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class testWithNoLockNoWith : TestModel
{
    public testWithNoLockNoWith()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/TestWithNoLockNoWith.sql");

        ExpectedProblems.Add(new TestProblem(4, 38, "Smells.SML003"));
    }

    [TestMethod]
    public void WithNoLockNoWith()
    {
        RunTest();
    }
}

