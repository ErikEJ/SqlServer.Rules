using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestTempTableWithNamedCheckConstraint : TestModel
{
    public TestTempTableWithNamedCheckConstraint()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/TempTableWithNamedCheckConstraint.sql");

        ExpectedProblems.Add(new TestProblem(14, 16, "Smells.SML040"));
    }

    [TestMethod]
    public void TempTableWithNamedCheckConstraint()
    {
        RunTest();
    }
}

