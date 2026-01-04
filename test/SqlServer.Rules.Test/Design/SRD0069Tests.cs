using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace SqlServer.Rules.Tests.Design;

[TestClass]
public class SRD0069Tests : TestModel
{
    public SRD0069Tests()
        : base(TestConstants.SqlServerRules)
    {
    }

    [TestMethod]
    public void RequireXactAbort()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/CreateProcedureExplicitTransaction.sql");

        ExpectedProblems.Add(new TestProblem(2, 1, "SqlServer.Rules.SRD0069"));

        RunTest();
    }

    [TestMethod]
    public void XactAbortSpecified()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/CreateProcedureExplicitTransaction2.sql");

        RunTest();
    }
}
