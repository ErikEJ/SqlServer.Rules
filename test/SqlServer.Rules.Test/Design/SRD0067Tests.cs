using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace SqlServer.Rules.Tests.Design;

[TestClass]
public class SRD0067Tests : TestModel
{
    public SRD0067Tests()
        : base(TestConstants.SqlServerRules)
    {
    }

    [TestMethod]
    public void Capitalize()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/KeywordCapitalize.sql");

        ExpectedProblems.Add(new TestProblem(7, 3, "SqlServer.Rules.SRD0067"));
        ExpectedProblems.Add(new TestProblem(9, 3, "SqlServer.Rules.SRD0067"));

        RunTest();
    }
}
