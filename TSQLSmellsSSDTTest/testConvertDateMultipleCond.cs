using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestConvertDateMultipleCond : TestModel
{
    public TestConvertDateMultipleCond()
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
