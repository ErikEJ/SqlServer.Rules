using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class testTempTableWithNamedFK : TestModel
{
    public testTempTableWithNamedFK()
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
#pragma warning restore IDE1006 // Naming Styles
