using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class testWithExistsAndNestedSelectStar : TestModel
{
    public testWithExistsAndNestedSelectStar()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/TestWithExistsAndNestedSelectStar.sql");

        ExpectedProblems.Add(new TestProblem(4, 18, "Smells.SML005"));
        ExpectedProblems.Add(new TestProblem(5, 9, "Smells.SML005"));
    }

    [TestMethod]
    public void WithExistsAndNestedSelectStar()
    {
        RunTest();
    }
}

