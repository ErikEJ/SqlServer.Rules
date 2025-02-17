using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestSelectFromTableVar : TestModel
{
    public TestSelectFromTableVar()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/SelectFromTableVar.sql");

        ExpectedProblems.Add(new TestProblem(9, 8, "Smells.SML005"));
        ExpectedProblems.Add(new TestProblem(4, 1, "Smells.SML033"));
    }

    [TestMethod]
    public void SelectFromTableVar()
    {
        RunTest();
    }
}

