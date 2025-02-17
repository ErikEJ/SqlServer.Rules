using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestTempTableWithNamedDefConstraint : TestModel
{
    public TestTempTableWithNamedDefConstraint()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/TempTableWithNamedDefConstraint.sql");

        ExpectedProblems.Add(new TestProblem(14, 3, "Smells.SML039"));
    }

    [TestMethod]
    public void TempTableWithNamedDefConstraint()
    {
        RunTest();
    }
}

