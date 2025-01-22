using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSQLSmellsSSDTTest.TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestWithExistsAndNestedSelectStarInlineIF : TestModel
{
    public TestWithExistsAndNestedSelectStarInlineIF()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/TestWithExistsAndNestedSelectStarInlineIF.sql");

        ExpectedProblems.Add(new TestProblem(4, 18, "Smells.SML005"));
        ExpectedProblems.Add(new TestProblem(4, 51, "Smells.SML005"));
    }

    [TestMethod]
    public void WithExistsAndNestedSelectStarInlineIF()
    {
        RunTest();
    }
}

