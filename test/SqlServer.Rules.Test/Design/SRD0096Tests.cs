using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace SqlServer.Rules.Tests.Design;

[TestClass]
public class SRD0096Tests : TestModel
{
    public SRD0096Tests()
        : base(TestConstants.SqlServerRules)
    {
    }

    [TestMethod]
    public void PotentialSqlInjectionDetected()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/PotentialSqlInjectionTest.sql");

        ExpectedProblems.Add(new TestProblem(8, 5, "SqlServer.Rules.SRD0096"));

        RunTest();
    }

    [TestMethod]
    public void PotentialSqlInjectionDetectedWhenAssignmentIgnored()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/PotentialSqlInjectionIgnorePropagationTest.sql");

        ExpectedProblems.Add(new TestProblem(9, 5, "SqlServer.Rules.SRD0096"));

        RunTest();
    }
}
