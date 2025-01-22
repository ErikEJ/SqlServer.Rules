using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSQLSmellsSSDTTest.TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestCreateProcedureNoSchema : TestModel
{
    public TestCreateProcedureNoSchema()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/CreateProcedureNoSchema.sql");

        ExpectedProblems.Add(new TestProblem(2, 18, "Smells.SML024"));
    }

    [TestMethod]
    public void CreateProcedureNoSchema()
    {
        RunTest();
    }
}

