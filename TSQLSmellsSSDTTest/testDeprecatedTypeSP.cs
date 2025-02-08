using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestDeprecatedTypeSP : TestModel
{
    public TestDeprecatedTypeSP()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/DeprecatedTypesSP.sql");
        ExpectedProblems.Add(new TestProblem(4, 14, "Smells.SML047"));
        ExpectedProblems.Add(new TestProblem(5, 14, "Smells.SML047"));
    }

    [TestMethod]
    public void DeprecatedTypesSP()
    {
        RunTest();
    }
}

