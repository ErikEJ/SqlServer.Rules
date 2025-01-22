using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSQLSmellsSSDTTest.TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestWithNoLock : TestModel
{
    public TestWithNoLock()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/TestWithNoLock.sql");

        ExpectedProblems.Add(new TestProblem(4, 42, "Smells.SML003"));
    }

    [TestMethod]
    public void WithNoLock()
    {
        RunTest();
    }
}

