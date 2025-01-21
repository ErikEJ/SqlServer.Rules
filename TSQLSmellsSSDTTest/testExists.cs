using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class testExists : TestModel
{
    public testExists()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/Exists.sql");

        // this._ExpectedProblems.Add(new TestProblem(7, 19, "Smells.SML025"));
    }

    [TestMethod]
    public void Exists()
    {
        RunTest();
    }
}

