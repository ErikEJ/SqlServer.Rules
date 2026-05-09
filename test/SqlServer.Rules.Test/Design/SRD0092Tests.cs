using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace SqlServer.Rules.Tests.Design;

[TestClass]
public class SRD0092Tests : TestModel
{
    public SRD0092Tests()
        : base(TestConstants.SqlServerRules)
    {
    }

    [TestMethod]
    public void NamedPKOnTempTableDetected()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/NamedPKOnTempTable.sql");

        ExpectedProblems.Add(new TestProblem(26, 9, "SqlServer.Rules.SRD0092"));

        RunTest();
    }
}
