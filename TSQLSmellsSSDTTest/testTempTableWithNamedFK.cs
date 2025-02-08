using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestTempTableWithNamedFK : TestModel
{
    public TestTempTableWithNamedFK()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/TempTableWithNamedFK.sql");

        // this._ExpectedProblems.Add(new TestProblem(14, 3, "Smells.SML040"));
    }

    [TestMethod]
    public void TempTableWithNamedFK()
    {
        RunTest();
    }
}

