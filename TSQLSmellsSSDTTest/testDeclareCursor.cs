using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestDeclareCursor : TestModel
{
    public TestDeclareCursor()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/DeclareCursor.sql");

        ExpectedProblems.Add(new TestProblem(5, 1, "Smells.SML029"));
    }

    [TestMethod]
    public void DeclareCursor()
    {
        RunTest();
    }
}

