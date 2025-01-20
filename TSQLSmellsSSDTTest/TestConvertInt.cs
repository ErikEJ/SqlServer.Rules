using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestConvertInt : TestModel
{
    public TestConvertInt()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/ConvertInt.sql");

        ExpectedProblems.Add(new TestProblem(7, 7, "Smells.SML006"));
    }

    [TestMethod]
    public void ConvertInt()
    {
        RunTest();
    }
}
#pragma warning restore IDE1006 // Naming Styles
