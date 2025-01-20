using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class testTempTableWithNamedCheckConstraint : TestModel
{
    public testTempTableWithNamedCheckConstraint()
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
#pragma warning restore IDE1006 // Naming Styles
