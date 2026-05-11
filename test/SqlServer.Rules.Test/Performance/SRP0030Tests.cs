using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.Rules.Tests.Helpers;

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
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/DeclareCursor.sql");

        ExpectedProblems.Add(new TestProblem(5, 1, "SqlServer.Rules.SRP0030"));

        RunTest();
    }

    [TestMethod]
    public void TestCursorWithFastForward()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/DeclareCursorFastForward.sql");

        RunTest();
    }
}
