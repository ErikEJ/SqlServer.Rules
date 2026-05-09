using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace SqlServer.Rules.Tests.Design;

[TestClass]
public class SRD0088Tests : TestModel
{
    public SRD0088Tests()
        : base(TestConstants.SqlServerRules)
    {
    }

    [TestMethod]
    public void NumericRoundAbortOnDetected()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/NumericRoundAbortOn.sql");

        ExpectedProblems.Add(new TestProblem(1, 1, "SqlServer.Rules.SRD0068"));
        ExpectedProblems.Add(new TestProblem(3, 1, "SqlServer.Rules.SRD0068"));
        ExpectedProblems.Add(new TestProblem(3, 1, "SqlServer.Rules.SRD0088"));
        ExpectedProblems.Add(new TestProblem(1, 1, "SqlServer.Rules.SRP0005"));

        RunTest();
    }

    [TestMethod]
    public void NumericRoundAbortOnNotPresent()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/SETs2.sql");

        ExpectedProblems.Add(new TestProblem(2, 1, "SqlServer.Rules.SRD0067"));
        ExpectedProblems.Add(new TestProblem(2, 1, "SqlServer.Rules.SRD0068"));
        ExpectedProblems.Add(new TestProblem(6, 1, "SqlServer.Rules.SRD0068"));
        ExpectedProblems.Add(new TestProblem(7, 1, "SqlServer.Rules.SRD0068"));
        ExpectedProblems.Add(new TestProblem(8, 1, "SqlServer.Rules.SRD0068"));
        ExpectedProblems.Add(new TestProblem(5, 1, "SqlServer.Rules.SRD0082"));
        ExpectedProblems.Add(new TestProblem(6, 1, "SqlServer.Rules.SRD0083"));
        ExpectedProblems.Add(new TestProblem(7, 1, "SqlServer.Rules.SRD0090"));

        RunTest();
    }

    [TestMethod]
    public void NumericRoundAbortOnInTriggerDetected()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/NumericRoundAbortOnTrigger.sql");

        ExpectedProblems.Add(new TestProblem(1, 1, "SqlServer.Rules.SRP0005"));
        ExpectedProblems.Add(new TestProblem(5, 1, "SqlServer.Rules.SRD0088"));

        RunTest();
    }
}
