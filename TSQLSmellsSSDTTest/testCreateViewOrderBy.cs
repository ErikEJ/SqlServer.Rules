using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestCreateViewOrderBy : TestModel
{
    public TestCreateViewOrderBy()
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

