using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class testInsertMissingColumnSpecifiers : TestModel
{
    public testInsertMissingColumnSpecifiers()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/InsertMissingColumnSpecifiers.sql");

        ExpectedProblems.Add(new TestProblem(5, 5, "Smells.SML012"));
    }

    [TestMethod]
    public void InsertMissingColumnSpecifiers()
    {
        RunTest();
    }
}
#pragma warning restore IDE1006 // Naming Styles
