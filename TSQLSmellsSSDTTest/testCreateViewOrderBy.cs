using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class testCreateViewOrderBy : TestModel
{
    public testCreateViewOrderBy()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/CreateViewOrderBy.sql");

        ExpectedProblems.Add(new TestProblem(5, 1, "Smells.SML028"));
    }

    [TestMethod]
    public void CreateViewOrderBy()
    {
        RunTest();
    }
}

