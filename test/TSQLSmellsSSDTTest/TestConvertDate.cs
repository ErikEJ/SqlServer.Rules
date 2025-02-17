using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestConvertDate : TestModel
{
    public TestConvertDate()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/ConvertDate.sql");

        ExpectedProblems.Add(new TestProblem(8, 7, "Smells.SML006"));
    }

    [TestMethod]
    public void ConvertDate()
    {
        RunTest();
    }
}

