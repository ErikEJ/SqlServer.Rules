using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestImplicitRangeWindow : TestModel
{
    public TestImplicitRangeWindow()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/ImplicitRangeWindow.sql");

        ExpectedProblems.Add(new TestProblem(5, 32, "Smells.SML026"));
    }

    [TestMethod]
    public void ImplicitRangeWindow()
    {
        RunTest();
    }
}

