using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSQLSmellsSSDTTest.TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class testWithExists : TestModel
{
    public testWithExists()
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

