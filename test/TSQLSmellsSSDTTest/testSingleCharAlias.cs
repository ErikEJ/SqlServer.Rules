using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestSingleCharAlias : TestModel
{
    public TestSingleCharAlias()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/SingleCharAlias.sql");

        ExpectedProblems.Add(new TestProblem(6, 8, "Smells.SML011"));
    }

    [TestMethod]
    public void SingleCharAlias()
    {
        RunTest();
    }
}

