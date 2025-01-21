using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class testInsertSelectStar : TestModel
{
    public testInsertSelectStar()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/InsertSelectStar.sql");

        ExpectedProblems.Add(new TestProblem(6, 9, "Smells.SML005"));
    }

    [TestMethod]
    public void InsertSelectStar()
    {
        RunTest();
    }
}

