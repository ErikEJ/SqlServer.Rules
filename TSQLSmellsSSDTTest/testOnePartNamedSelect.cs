using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class testOnePartNamedSelect : TestModel
{
    public testOnePartNamedSelect()
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

