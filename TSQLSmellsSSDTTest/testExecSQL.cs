using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class testExecSQL : TestModel
{
    public testExecSQL()
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

