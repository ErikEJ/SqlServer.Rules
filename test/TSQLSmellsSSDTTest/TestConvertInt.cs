using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestConvertInt : TestModel
{
    public TestConvertInt()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/ConvertInt.sql");

        ExpectedProblems.Add(new TestProblem(7, 7, "Smells.SML006"));
    }

    [TestMethod]
    public void ConvertInt()
    {
        RunTest();
    }
}

