using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestSetNoCountON : TestModel
{
    public TestSetNoCountON()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/SetNoCountON.sql");

        // this._ExpectedProblems.Add(new TestProblem(5, 9, "Smells.SML034"));
    }

    [TestMethod]
    public void SetNoCountON()
    {
        RunTest();
    }
}

