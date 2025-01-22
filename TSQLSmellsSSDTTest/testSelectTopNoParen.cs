using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSQLSmellsSSDTTest.TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class testSelectTopNoParen : TestModel
{
    public testSelectTopNoParen()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/SelectTopNoParen.sql");

        ExpectedProblems.Add(new TestProblem(5, 9, "Smells.SML034"));
    }

    [TestMethod]
    public void SelectTopNoParen()
    {
        RunTest();
    }
}

