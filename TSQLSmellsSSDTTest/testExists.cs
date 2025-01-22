using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSQLSmellsSSDTTest.TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestExists : TestModel
{
    public TestExists()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/Exists.sql");

        // this._ExpectedProblems.Add(new TestProblem(7, 19, "Smells.SML025"));
    }

    [TestMethod]
    public void Exists()
    {
        RunTest();
    }
}

