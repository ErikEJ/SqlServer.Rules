using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestInsertSelectStar : TestModel
{
    public TestInsertSelectStar()
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

