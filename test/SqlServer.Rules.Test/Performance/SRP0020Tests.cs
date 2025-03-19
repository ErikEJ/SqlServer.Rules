using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace SqlServer.Rules.Tests.Design;

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
