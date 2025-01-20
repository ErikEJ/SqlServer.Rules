using Microsoft.VisualStudio.TestTools.UnitTesting;

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
#pragma warning restore IDE1006 // Naming Styles
