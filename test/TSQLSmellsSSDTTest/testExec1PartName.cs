using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestExec1PartName : TestModel
{
    public TestExec1PartName()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/Exec1PartName.sql");

        ExpectedProblems.Add(new TestProblem(5, 6, "Smells.SML021"));
    }

    [TestMethod]
    public void Exec1PartName()
    {
        RunTest();
    }
}

