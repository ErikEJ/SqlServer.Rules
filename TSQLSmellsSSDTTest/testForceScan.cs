using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestForceScan : TestModel
{
    public TestForceScan()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/ForceScan.sql");

        ExpectedProblems.Add(new TestProblem(6, 30, "Smells.SML044"));
    }

    [TestMethod]
    public void ForceScan()
    {
        RunTest();
    }
}

