using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace SqlServer.Rules.Tests.Design;

[TestClass]
public class SRD0071Tests : TestModel
{
    public SRD0071Tests()
        : base(TestConstants.SqlServerRules)
    {
    }

    [TestMethod]
    public void CaseWithoutElseDetected()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/CaseWithoutElseTest.sql");

        ExpectedProblems.Add(new TestProblem(8, 18, "SqlServer.Rules.SRD0071"));
        ExpectedProblems.Add(new TestProblem(14, 21, "SqlServer.Rules.SRD0071"));

        RunTest();
    }

    [TestMethod]
    public void CaseWithElseClean()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/CaseWithElseTest.sql");

        RunTest();
    }
}
