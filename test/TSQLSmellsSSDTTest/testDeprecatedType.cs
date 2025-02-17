using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestDeprecatedType : TestModel
{
    public TestDeprecatedType()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/DeprecatedTypes.sql");
        ExpectedProblems.Add(new TestProblem(4, 16, "Smells.SML047"));
    }

    [TestMethod]
    public void DeprecatedTypes()
    {
        RunTest();
    }
}

