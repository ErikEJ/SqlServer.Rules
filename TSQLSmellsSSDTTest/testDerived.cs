using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class testDerived : TestModel
{
    public testDerived()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/Derived.sql");

        ExpectedProblems.Add(new TestProblem(7, 24, "Smells.SML035"));
    }

    [TestMethod]
    public void Derived()
    {
        RunTest();
    }
}
#pragma warning restore IDE1006 // Naming Styles
