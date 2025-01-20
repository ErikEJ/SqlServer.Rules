using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class testConvertDateMultipleCond : TestModel
{
    public testConvertDateMultipleCond()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/ConvertDateMultiCond.sql");

        ExpectedProblems.Add(new TestProblem(7, 7, "Smells.SML006"));
        ExpectedProblems.Add(new TestProblem(8, 5, "Smells.SML006"));
    }

    [TestMethod]
    public void ConvertDateMultipleCond()
    {
        RunTest();
    }
}
#pragma warning restore IDE1006 // Naming Styles
