using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class testTempTableWithNamedPK : TestModel
{
    public testTempTableWithNamedPK()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/TempTableWithNamedPK.sql");

        ExpectedProblems.Add(new TestProblem(14, 3, "Smells.SML038"));

        // this._ExpectedProblems.Add(new TestProblem(7, 8, "Smells.SML005"));
        // this._ExpectedProblems.Add(new TestProblem(9, 8, "Smells.SML005"));
    }

    [TestMethod]
    public void TempTableWithNamedPK()
    {
        RunTest();
    }
}

