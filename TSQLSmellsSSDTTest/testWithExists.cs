using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestWithExists : TestModel
{
    public TestWithExists()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/TestWithExists.sql");

        ExpectedProblems.Add(new TestProblem(5, 18, "Smells.SML005"));
    }

    [TestMethod]
    public void WithExists()
    {
        RunTest();
    }
}

