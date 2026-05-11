using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.Rules.Tests.Helpers;

namespace SqlServer.Rules.Tests.Performance;

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
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/UpperFunction.sql");

        ExpectedProblems.Add(new TestProblem(6, 7, "SqlServer.Rules.SRP0009"));

        RunTest();
    }
}
