using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class testImplicitRangeWindow : TestModel
{
    public testImplicitRangeWindow()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/ImplicitRangeWindow.sql");

        ExpectedProblems.Add(new TestProblem(5, 32, "Smells.SML026"));
    }

    [TestMethod]
    public void ImplicitRangeWindow()
    {
        RunTest();
    }
}

