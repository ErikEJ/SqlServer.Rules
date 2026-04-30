using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace SqlServer.Rules.Tests.Design;

[TestClass]
public class SRD0078Tests : TestModel
{
    public SRD0078Tests()
        : base(TestConstants.SqlServerRules)
    {
    }

    [TestMethod]
    public void SingleCharacterAliasDetected()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/SingleCharAliasTest.sql");

        ExpectedProblems.Add(new TestProblem(6, 10, "SqlServer.Rules.SRD0078"));

        RunTest();
    }
}
