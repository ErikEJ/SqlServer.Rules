using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class testDeclareCursor : TestModel
{
    public testDeclareCursor()
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
#pragma warning restore IDE1006 // Naming Styles
