using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

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

        ExpectedProblems.Add(new TestProblem(4, 1, "SqlServer.Rules.SRD0089"));

        RunTest();
    }
}
