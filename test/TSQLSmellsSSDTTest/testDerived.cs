using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class TestDerived : TestModel
{
    public TestDerived()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/Derived.sql");

        ExpectedProblems.Add(new TestProblem(7, 24, "Smells.SML035"));
    }

    [TestMethod]
    public void Derived()
    {
        RunTest();
    }
}

