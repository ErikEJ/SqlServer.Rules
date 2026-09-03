using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.Rules.Tests.Helpers;

namespace SqlServer.Rules.Tests.Design;

[TestClass]
public class SRD0062Tests : TestModel
{
    public SRD0062Tests()
        : base(TestConstants.SqlServerRules)
    {
    }

    [TestMethod]
    public void AliasTypeAndXmlTempTableColumnsDoNotThrow()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/SRD0062AliasTypeAndXmlTempTable.sql");
        ExpectedProblems.Add(new TestProblem(10, 9, "SqlServer.Rules.SRD0062"));

        RunTest();
    }
}
