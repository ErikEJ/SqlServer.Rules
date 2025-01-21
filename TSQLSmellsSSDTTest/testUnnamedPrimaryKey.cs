using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class testUnnamedPrimaryKey : TestModel
{
    public testUnnamedPrimaryKey()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/UnnamedPK.sql");

        // this._ExpectedProblems.Add(new TestProblem(5, 8, "Smells.SML005"));
        // this._ExpectedProblems.Add(new TestProblem(7, 8, "Smells.SML005"));
        // this._ExpectedProblems.Add(new TestProblem(9, 8, "Smells.SML005"));
    }

    [TestMethod]
    public void UnnamedPrimaryKey()
    {
        RunTest();
    }
}

