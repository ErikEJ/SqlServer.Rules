using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class testsqlInjection : TestModel
{
    public testsqlInjection()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/Inject.sql");

        ExpectedProblems.Add(new TestProblem(14, 10, "Smells.SML043"));
        ExpectedProblems.Add(new TestProblem(23, 10, "Smells.SML043"));
        ExpectedProblems.Add(new TestProblem(52, 10, "Smells.SML043"));
        ExpectedProblems.Add(new TestProblem(88, 10, "Smells.SML043"));
        ExpectedProblems.Add(new TestProblem(5, 7, "Smells.SML043"));
    }

    [TestMethod]
    public void SQLInjection()
    {
        RunTest();
    }
}

