using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace SqlServer.Rules.Tests.Performance;

[TestClass]
public class SRP0030Tests : TestModel
{
    public SRP0030Tests()
        : base(TestConstants.SqlServerRules)
    {
    }

    [TestMethod]
    public void TestCursorWithoutFastForward()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/CursorSpecifyFastForward.sql");

        ExpectedProblems.Add(new TestProblem(5, 1, "SqlServer.Rules.SRP0030"));

        RunTest();
    }
}
