using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestConvertInt2 : TestModel
{
    public TestConvertInt2()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/ConvertInt2.sql");

        ExpectedProblems.Add(new TestProblem(7, 14, "Smells.SML006"));
    }

    [TestMethod]
    public void ConvertInt2()
    {
        RunTest();
    }
}

