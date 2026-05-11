using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.Rules.Tests.Helpers;

namespace SqlServer.Rules.Tests.Design;

[TestClass]
public class SRD0080Tests : TestModel
{
    public SRD0080Tests()
        : base(TestConstants.SqlServerRules)
    {
    }

    [TestMethod]
    public void TopExpressionWithoutParenthesesDetected()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/TopExpressionParenthesesTest.sql");

        ExpectedProblems.Add(new TestProblem(5, 12, "SqlServer.Rules.SRD0080"));

        RunTest();
    }
}
