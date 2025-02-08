using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestInsertMissingColumnSpecifiers : TestModel
{
    public TestInsertMissingColumnSpecifiers()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/InsertMissingColumnSpecifiers.sql");

        ExpectedProblems.Add(new TestProblem(5, 5, "Smells.SML012"));
    }

    [TestMethod]
    public void InsertMissingColumnSpecifiers()
    {
        RunTest();
    }
}

