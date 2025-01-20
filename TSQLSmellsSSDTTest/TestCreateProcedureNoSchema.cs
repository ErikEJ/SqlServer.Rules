using Microsoft.VisualStudio.TestTools.UnitTesting;

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
#pragma warning restore IDE1006 // Naming Styles
