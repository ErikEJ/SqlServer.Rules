using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class testCreateTableNoSchema : TestModel
{
    public testCreateTableNoSchema()
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

