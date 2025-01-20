using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TSQLSmellsSSDTTest;

#pragma warning disable IDE1006 // Naming Styles
[TestClass]
public class TestConvertDate : TestModel
{
    public TestConvertDate()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/ConvertDate.sql");

        ExpectedProblems.Add(new TestProblem(8, 7, "Smells.SML006"));
    }

    [TestMethod]
    public void ConvertDate()
    {
        RunTest();
    }
}
#pragma warning restore IDE1006 // Naming Styles
