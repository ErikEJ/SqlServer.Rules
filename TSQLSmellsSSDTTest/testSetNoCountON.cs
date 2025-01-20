using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class testSetNoCountON : TestModel
{
    public testSetNoCountON()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/SetNoCountON.sql");

        // this._ExpectedProblems.Add(new TestProblem(5, 9, "Smells.SML034"));
    }

    [TestMethod]
    public void SetNoCountON()
    {
        RunTest();
    }
}
#pragma warning restore IDE1006 // Naming Styles
