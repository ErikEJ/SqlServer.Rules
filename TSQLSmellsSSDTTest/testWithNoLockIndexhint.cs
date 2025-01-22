using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSQLSmellsSSDTTest.TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestWithNoLockIndexhint : TestModel
{
    public TestWithNoLockIndexhint()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/TestWithNoLockIndexhint.sql");

        ExpectedProblems.Add(new TestProblem(4, 42, "Smells.SML003"));
        ExpectedProblems.Add(new TestProblem(4, 49, "Smells.SML045"));
    }

    [TestMethod]
    public void WithNoLockIndexhint()
    {
        RunTest();
    }
}

