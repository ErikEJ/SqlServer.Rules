using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestOnePartNamedSelect : TestModel
{
    public TestOnePartNamedSelect()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/TestOnePartNamedSelect.sql");

        ExpectedProblems.Add(new TestProblem(6, 19, "Smells.SML002"));
    }

    [TestMethod]
    public void OnePartNamedSelect()
    {
        RunTest();
    }
}

