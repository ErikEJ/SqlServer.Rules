using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSQLSmellsSSDTTest.TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestOnePartNamedSelect : TestModel
{
    public TestOnePartNamedSelect()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/TestOnePartNamedSelect.sql");

        ExpectedProblems.Add(new TestProblem(6, 19, "Smells.SML002"));
    }

    [TestMethod]
    public void OnePartNamedSelect()
    {
        RunTest();
    }
}

