using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSQLSmellsSSDTTest.TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class testSetTransactionIsolationLevel : TestModel
{
    public testSetTransactionIsolationLevel()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/SetTransactionIsolationLevel.sql");

        ExpectedProblems.Add(new TestProblem(5, 1, "Smells.SML010"));
    }

    [TestMethod]
    public void SetTransactionIsolationLevel()
    {
        RunTest();
    }
}

