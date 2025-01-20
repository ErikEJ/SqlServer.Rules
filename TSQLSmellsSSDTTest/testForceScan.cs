using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class testForceScan : TestModel
{
    public testForceScan()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/ForceScan.sql");

        ExpectedProblems.Add(new TestProblem(6, 30, "Smells.SML044"));
    }

    [TestMethod]
    public void ForceScan()
    {
        RunTest();
    }
}
#pragma warning restore IDE1006 // Naming Styles
