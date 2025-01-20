using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class testSets2 : TestModel
{
    public testSets2()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/SETs2.sql");

        ExpectedProblems.Add(new TestProblem(5, 16, "Smells.SML008"));
        ExpectedProblems.Add(new TestProblem(6, 15, "Smells.SML009"));
        ExpectedProblems.Add(new TestProblem(7, 1, "Smells.SML020"));
        ExpectedProblems.Add(new TestProblem(8, 1, "Smells.SML022"));
    }

    [TestMethod]
    public void Sets2()
    {
        RunTest();
    }
}
#pragma warning restore IDE1006 // Naming Styles
