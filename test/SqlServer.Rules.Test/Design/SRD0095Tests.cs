using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace SqlServer.Rules.Tests.Design;

[TestClass]
public class SRD0095Tests : TestModel
{
    public SRD0095Tests()
        : base(TestConstants.SqlServerRules)
    {
    }

    [TestMethod]
    public void NamedCheckConstraintOnTempTableDetected()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/SRD0095TempTableWithNamedCheckConstraint.sql");

        ExpectedProblems.Add(new TestProblem(13, 18, "SqlServer.Rules.SRD0095"));
        ExpectedProblems.Add(new TestProblem(15, 5, "SqlServer.Rules.SRD0095"));

        RunTest();
    }
}
