using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestExecSQL : TestModel
{
    public TestExecSQL()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/ExecSQL.sql");

        ExpectedProblems.Add(new TestProblem(6, 1, "Smells.SML012"));
    }

    [TestMethod]
    public void ExecSQL()
    {
        RunTest();
    }
}

