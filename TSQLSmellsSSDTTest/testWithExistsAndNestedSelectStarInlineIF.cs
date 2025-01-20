using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class testWithExistsAndNestedSelectStarInlineIF : TestModel
{
    public testWithExistsAndNestedSelectStarInlineIF()
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
#pragma warning restore IDE1006 // Naming Styles
