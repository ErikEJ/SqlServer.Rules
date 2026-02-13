using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace SqlServer.Rules.Tests.Design;

[TestClass]
public class SRD0020Tests : TestModel
{
    public SRD0020Tests()
        : base(TestConstants.SqlServerRules)
    {
    }

    [TestMethod]
    public void MissingJoinRuleBug()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/SRD0020_vw_Repro.sql");

        RunTest();
    }
}
