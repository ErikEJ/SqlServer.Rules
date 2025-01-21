using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class testExec1PartName : TestModel
{
    public testExec1PartName()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/Exec1PartName.sql");

        ExpectedProblems.Add(new TestProblem(5, 6, "Smells.SML021"));
    }

    [TestMethod]
    public void Exec1PartName()
    {
        RunTest();
    }
}

