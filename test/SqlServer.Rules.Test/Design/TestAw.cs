using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

namespace SqlServer.Rules.Tests.Design;

[TestClass]
public class TestAW : TestModel
{
    public TestAW()
       : base(TestConstants.SqlServerRules)
    {
    }

    [TestMethod]
    [Ignore("Ignore AdventureWorks test")]
    public void TestAdventureworksWithSqlServerRules()
    {
        foreach (var fileName in Directory.GetFiles("../../../../../sqlprojects/AW/Tables", "*.sql"))
        {
            TestFiles.Add(fileName);
        }

        RunTest();
    }
}
