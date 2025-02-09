using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace SqlServer.Rules.Tests.Design;

[TestClass]
public class SRP0009Tests : TestModel
{
    public SRP0009Tests()
        : base(TestConstants.SqlServerRules)
    {
    }

    [TestMethod]
    public void UpperFunctionInWhere()
    {
        TestFiles.Add("../../../../TSQLSmellsTest/UpperFunction.sql");

        ExpectedProblems.Add(new TestProblem(6, 7, "SqlServer.Rules.SRP0009"));

        RunTest();
    }
}
