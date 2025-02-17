using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestTableHints : TestModel
{
    public TestTableHints()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/TableHints.sql");

        ExpectedProblems.Add(new TestProblem(5, 1, "Smells.SML004"));
    }

    [TestMethod]
    public void TableHints()
    {
        RunTest();
    }
}

