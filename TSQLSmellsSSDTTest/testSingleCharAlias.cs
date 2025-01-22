using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSQLSmellsSSDTTest.TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class testSingleCharAlias : TestModel
{
    public testSingleCharAlias()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/SingleCharAlias.sql");

        ExpectedProblems.Add(new TestProblem(6, 8, "Smells.SML011"));
    }

    [TestMethod]
    public void SingleCharAlias()
    {
        RunTest();
    }
}

