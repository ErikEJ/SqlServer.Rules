using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace SqlServer.Rules.Tests.Design;

[TestClass]
public class SRD0075Tests : TestModel
{
    public SRD0075Tests()
        : base(TestConstants.SqlServerRules)
    {
    }

    [TestMethod]
    public void HardCodedCredentialsDetected()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/HardCodedCredentialsTest.sql");

        ExpectedProblems.Add(new TestProblem(5, 13, "SqlServer.Rules.SRD0012"));
        ExpectedProblems.Add(new TestProblem(5, 13, "SqlServer.Rules.SRD0075"));
        ExpectedProblems.Add(new TestProblem(7, 5, "SqlServer.Rules.SRD0075"));

        RunTest();
    }
}
