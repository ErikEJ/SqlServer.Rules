using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class testConvertInt2 : TestModel
{
    public testConvertInt2()
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

