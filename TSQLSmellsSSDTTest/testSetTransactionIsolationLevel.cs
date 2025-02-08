using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestSetTransactionIsolationLevel : TestModel
{
    public TestSetTransactionIsolationLevel()
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

