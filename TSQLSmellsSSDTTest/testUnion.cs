using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestUnion : TestModel
{
    public TestUnion()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/UnionTest.sql");

        ExpectedProblems.Add(new TestProblem(5, 8, "Smells.SML005"));
        ExpectedProblems.Add(new TestProblem(7, 8, "Smells.SML005"));
        ExpectedProblems.Add(new TestProblem(9, 8, "Smells.SML005"));
    }

    [TestMethod]
    public void Union()
    {
        RunTest();
    }
}

