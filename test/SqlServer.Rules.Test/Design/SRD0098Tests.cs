using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.Rules.Tests.Helpers;

namespace SqlServer.Rules.Tests.Design;

[TestClass]
public class SRD0098Tests : TestModel
{
    public SRD0098Tests()
        : base(TestConstants.SqlServerRules)
    {
    }

    [TestMethod]
    public void FlagsColumnsWithoutExplicitNullability()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/SRD0098ExplicitColumnNullability.sql");

        ExpectedProblems.Add(new TestProblem(5, 5, "SqlServer.Rules.SRD0098"));

        RunTest();
    }
}
