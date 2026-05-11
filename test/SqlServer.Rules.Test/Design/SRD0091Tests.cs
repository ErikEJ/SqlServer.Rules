using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.Rules.Tests.Helpers;

namespace SqlServer.Rules.Tests.Design;

[TestClass]
public class SRD0091Tests : TestModel
{
    public SRD0091Tests()
        : base(TestConstants.SqlServerRules)
    {
    }

    [TestMethod]
    public void OrderByInDerivedTableDetected()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/OrderByInDerivedTableTest.sql");

        ExpectedProblems.Add(new TestProblem(11, 9, "SqlServer.Rules.SRD0091"));

        RunTest();
    }
}
