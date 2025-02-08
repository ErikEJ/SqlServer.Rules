using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestLagFunction : TestModel
{
    public TestLagFunction()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/LAGFunction.sql");

        // this._ExpectedProblems.Add(new TestProblem(6, 9, "Smells.SML005"));
    }

    [TestMethod]
    public void LagFunction()
    {
        RunTest();
    }
}

