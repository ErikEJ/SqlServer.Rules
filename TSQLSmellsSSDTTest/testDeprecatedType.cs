using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TSQLSmellsSSDTTest;

[TestClass]
public class testDeprecatedType : TestModel
{
    public testDeprecatedType()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/DeprecatedTypes.sql");
        ExpectedProblems.Add(new TestProblem(4, 16, "Smells.SML047"));
    }

    [TestMethod]
    public void DeprecatedTypes()
    {
        RunTest();
    }
}
#pragma warning restore IDE1006 // Naming Styles
