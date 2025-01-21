using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class testLagFunction : TestModel
{
    public testLagFunction()
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

