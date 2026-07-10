using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.Rules.Tests.Helpers;

namespace SqlServer.Rules.Tests.Design;

[TestClass]
public class SRD0039Tests : TestModel
{
    public SRD0039Tests()
        : base(TestConstants.SqlServerRules)
    {
    }

    [TestMethod]
    public void IgnoreCteAlias()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/CteAlias.sql");

        ExpectedProblems.Add(new TestProblem(1, 1, "SqlServer.Rules.SRD0068"));
        ExpectedProblems.Add(new TestProblem(3, 1, "SqlServer.Rules.SRD0068"));
        ExpectedProblems.Add(new TestProblem(1, 1, "SqlServer.Rules.SRP0005"));

        RunTest();
    }

    [TestMethod]
    public void IgnoreCteReferenceAfterEarlierSelect()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/CteJoinAfterEarlierSelect.sql");

        ExpectedProblems.Add(new TestProblem(1, 1, "SqlServer.Rules.SRP0005"));
        ExpectedProblems.Add(new TestProblem(10, 12, "SqlServer.Rules.SRD0038"));

        RunTest();
    }
}
