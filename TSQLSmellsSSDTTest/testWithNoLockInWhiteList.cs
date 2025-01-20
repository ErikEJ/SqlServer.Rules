using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class testWithNoLockInWhiteList : TestModel
{
    public testWithNoLockInWhiteList()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/TestWithNoLockInWhiteList.sql");

        // this._ExpectedProblems.Add(new TestProblem(4, 42, "Smells.SML003"));
    }

    [TestMethod]
    public void WithNoLockInWhiteList()
    {
        RunTest();
    }
}
#pragma warning restore IDE1006 // Naming Styles
