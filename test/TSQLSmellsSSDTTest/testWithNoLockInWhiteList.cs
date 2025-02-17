using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestWithNoLockInWhiteList : TestModel
{
    public TestWithNoLockInWhiteList()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/TestWithNoLockInWhiteList.sql");

        // this._ExpectedProblems.Add(new TestProblem(4, 42, "Smells.SML003"));
    }

    [TestMethod]
    public void WithNoLockInWhiteList()
    {
        RunTest();
    }
}

