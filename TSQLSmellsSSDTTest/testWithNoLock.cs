using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class testWithNoLock : TestModel
{
    public testWithNoLock()
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

