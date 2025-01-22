using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSQLSmellsSSDTTest.TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestMultiCteTest : TestModel
{
    public TestMultiCteTest()
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

