using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class testSingleLineComment : TestModel
{
    public testSingleLineComment()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/SingleLineComment.sql");

        // this._ExpectedProblems.Add(new TestProblem(5, 8, "Smells.SML005"));
        // this._ExpectedProblems.Add(new TestProblem(7, 8, "Smells.SML005"));
        // this._ExpectedProblems.Add(new TestProblem(9, 8, "Smells.SML005"));
    }

    [TestMethod]
    public void SingleLineComment()
    {
        RunTest();
    }
}

