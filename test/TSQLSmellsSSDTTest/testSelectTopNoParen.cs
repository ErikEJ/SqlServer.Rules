using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestSelectTopNoParen : TestModel
{
    public TestSelectTopNoParen()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/SelectTopNoParen.sql");

        ExpectedProblems.Add(new TestProblem(5, 9, "Smells.SML034"));
    }

    [TestMethod]
    public void SelectTopNoParen()
    {
        RunTest();
    }
}

