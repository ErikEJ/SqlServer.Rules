using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace SqlServer.Rules.Tests.Design;

[TestClass]
public class SRD0093Tests : TestModel
{
    public SRD0093Tests()
        : base(TestConstants.SqlServerRules)
    {
    }

    [TestMethod]
    public void NamedDefaultConstraintOnTempTableDetected()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/SRD0093TempTableNamedDefault.sql");

        ExpectedProblems.Add(new TestProblem(13, 9, "SqlServer.Rules.SRD0093"));

        RunTest();
    }
}
