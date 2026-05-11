using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.Rules.Tests.Helpers;

namespace SqlServer.Rules.Tests.Performance;

[TestClass]
public class SRP0020Tests : TestModel
{
    public SRP0020Tests()
        : base(TestConstants.SqlServerRules)
    {
    }

    [TestMethod]
    public void HasClusteredIndex()
    {
        TestFiles.Add("../../../../../sqlprojects/TSQLSmellsTest/CreateTableClusteredColumnStore.sql");

        RunTest();
    }
}
