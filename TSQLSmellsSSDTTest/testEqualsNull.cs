using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestEqualsNull : TestModel
{
    public TestEqualsNull()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/EqualsNull.sql");
        ExpectedProblems.Add(new TestProblem(13, 39, "Smells.SML046"));
    }

    [TestMethod]
    public void EqualsNull()
    {
        RunTest();
    }
}

