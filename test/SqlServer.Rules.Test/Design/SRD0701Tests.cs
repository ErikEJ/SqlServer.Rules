using System;
using System.Collections.Generic;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.Rules.Design;
using SqlServer.Rules.Tests.Utils;

namespace SqlServer.Rules.Tests.Design;

[TestClass]
[TestCategory("Design")]
public class SRD0701Tests : TestCasesBase
{
    [TestMethod]
    public void QueryStoreNotReadWriteDetected()
    {
        var options = new TSqlModelOptions { QueryStoreDesiredState = QueryStoreDesiredState.ReadOnly };
        using var test = new RuleTest(new List<Tuple<string, string>>(), options, SqlVersion);
        test.RunTest(QueryStoreReadWriteRule.RuleId, (result, _) =>
        {
            Assert.AreEqual(1, result.Problems.Count, "Expected 1 problem when Query Store is not READ_WRITE");
            Assert.IsTrue(result.Problems[0].Description.Contains(QueryStoreReadWriteRule.Message, StringComparison.Ordinal));
        });
    }

    [TestMethod]
    public void QueryStoreReadWriteNotDetected()
    {
        var options = new TSqlModelOptions { QueryStoreDesiredState = QueryStoreDesiredState.ReadWrite };
        using var test = new RuleTest(new List<Tuple<string, string>>(), options, SqlVersion);
        test.RunTest(QueryStoreReadWriteRule.RuleId, (result, _) =>
        {
            Assert.AreEqual(0, result.Problems.Count, "Expected 0 problems when Query Store is READ_WRITE");
        });
    }

    [TestMethod]
    public void QueryStoreSql2016Detected()
    {
        var options = new TSqlModelOptions();
        using var test = new RuleTest(new List<Tuple<string, string>>(), options, SqlServerVersion.Sql130);
        test.RunTest(QueryStoreReadWriteRule.RuleId, (result, _) =>
        {
            Assert.AreEqual(1, result.Problems.Count, "Expected 1 problem for SQL Server 2016 targets when Query Store is not READ_WRITE");
        });
    }

    [TestMethod]
    public void QueryStorePreSql2016Ignored()
    {
        var options = new TSqlModelOptions();
        using var test = new RuleTest(new List<Tuple<string, string>>(), options, SqlServerVersion.Sql120);
        test.RunTest(QueryStoreReadWriteRule.RuleId, (result, _) =>
        {
            Assert.AreEqual(0, result.Problems.Count, "Expected 0 problems for SQL Server targets before 2016");
        });
    }

    [TestMethod]
    public void QueryStoreAzureSqlIgnored()
    {
        var options = new TSqlModelOptions();
        using var test = new RuleTest(new List<Tuple<string, string>>(), options, SqlServerVersion.SqlAzure);
        test.RunTest(QueryStoreReadWriteRule.RuleId, (result, _) =>
        {
            Assert.AreEqual(0, result.Problems.Count, "Expected 0 problems for Azure SQL Database target");
        });
    }
}
