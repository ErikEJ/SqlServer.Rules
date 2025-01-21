using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class testTableHints : TestModel
{
    public testTableHints()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/TableHints.sql");

        ExpectedProblems.Add(new TestProblem(5, 1, "Smells.SML004"));
    }

    [TestMethod]
    public void TableHints()
    {
        RunTest();
    }
}

