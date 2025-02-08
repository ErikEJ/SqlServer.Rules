using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestCreateTableNoSchema : TestModel
{
    public TestCreateTableNoSchema()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/CreateTableNoSchema.sql");

        ExpectedProblems.Add(new TestProblem(1, 1, "Smells.SML027"));
    }

    [TestMethod]
    public void CreateTableNoSchema()
    {
        RunTest();
    }
}

