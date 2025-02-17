using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestCreateProcedureNoSchema : TestModel
{
    public TestCreateProcedureNoSchema()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/CreateProcedureNoSchema.sql");

        ExpectedProblems.Add(new TestProblem(2, 18, "Smells.SML024"));
    }

    [TestMethod]
    public void CreateProcedureNoSchema()
    {
        RunTest();
    }
}

