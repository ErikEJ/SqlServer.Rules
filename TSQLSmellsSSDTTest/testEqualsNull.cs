using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class testEqualsNull : TestModel
{
    public testEqualsNull()
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

