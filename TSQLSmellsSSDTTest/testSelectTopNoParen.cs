using Microsoft.VisualStudio.TestTools.UnitTesting;

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
#pragma warning restore IDE1006 // Naming Styles
