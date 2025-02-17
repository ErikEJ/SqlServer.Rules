using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestWithExistsAndNestedSelectStar : TestModel
{
    public TestWithExistsAndNestedSelectStar()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/TestWithExistsAndNestedSelectStar.sql");

        ExpectedProblems.Add(new TestProblem(4, 18, "Smells.SML005"));
        ExpectedProblems.Add(new TestProblem(5, 9, "Smells.SML005"));
    }

    [TestMethod]
    public void WithExistsAndNestedSelectStar()
    {
        RunTest();
    }
}

