using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.Rules.Tests.Helpers;

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

    [TestMethod]
    public void PotentialSqlInjectionDetectedForExecConcatenation()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/PotentialSqlInjectionExecConcatTest.sql");

        ExpectedProblems.Add(new TestProblem(6, 5, "SqlServer.Rules.SRD0096"));
        ExpectedProblems.Add(new TestProblem(6, 11, "SqlServer.Rules.SRD0024"));

        RunTest();
    }

    [TestMethod]
    public void PotentialSqlInjectionDetectedForPositionalSpExecuteSql()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/PotentialSqlInjectionPositionalSpExecuteSqlTest.sql");

        ExpectedProblems.Add(new TestProblem(7, 5, "SqlServer.Rules.SRD0096"));
        ExpectedProblems.Add(new TestProblem(7, 5, "SqlServer.Rules.SRD0058"));

        RunTest();
    }

    [TestMethod]
    public void PotentialSqlInjectionDetectedWhenDeclareAssignmentIgnored()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/PotentialSqlInjectionIgnoreDeclarePropagationTest.sql");

        ExpectedProblems.Add(new TestProblem(8, 5, "SqlServer.Rules.SRD0096"));

        RunTest();
    }
}
