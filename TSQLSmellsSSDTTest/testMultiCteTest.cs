using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class testMultiCteTest : TestModel
{
    public testMultiCteTest()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/MultiCteTest.sql");

        ExpectedProblems.Add(new TestProblem(8, 10, "Smells.SML005"));
    }

    [TestMethod]
    public void MultiCteTest()
    {
        RunTest();
    }
}
#pragma warning restore IDE1006 // Naming Styles
