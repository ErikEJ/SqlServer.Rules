using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.Rules.Tests.Helpers;

namespace SqlServer.Rules.Tests.Design;

[TestClass]
public class SRD0089Tests : TestModel
{
    public SRD0089Tests()
        : base(TestConstants.SqlServerRules)
    {
    }

    [TestMethod]
    public void QuotedIdentifierOffDetected()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/SRD0089_QuotedIdentifierOff.sql");

        ExpectedProblems.Add(new TestProblem(1, 1, "SqlServer.Rules.SRD0068"));
        ExpectedProblems.Add(new TestProblem(4, 1, "SqlServer.Rules.SRD0089"));
        ExpectedProblems.Add(new TestProblem(1, 1, "SqlServer.Rules.SRP0005"));

        RunTest();
    }
}
