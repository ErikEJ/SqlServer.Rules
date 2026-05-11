using System;
using System.Collections.Generic;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.Rules.Design;
using SqlServer.Rules.Tests.Utils;

namespace SqlServer.Rules.Tests.Design;

[TestClass]
[TestCategory("Design")]
public class SRD0703Tests : TestCasesBase
{
    [TestMethod]
    public void QueryStoreCaptureModeNotAutoDetected()
    {
        var options = new TSqlModelOptions();
        using var test = new RuleTest(new List<Tuple<string, string>>(), options, SqlVersion);
        test.RunTest(QueryStoreCaptureModeAutoRule.RuleId, (result, _) =>
        {
            Assert.AreEqual(1, result.Problems.Count, "Expected 1 problem when QUERY_STORE_CAPTURE_MODE is not AUTO");
            Assert.IsTrue(result.Problems[0].Description.Contains(QueryStoreCaptureModeAutoRule.Message, StringComparison.Ordinal));
        });
    }

    [TestMethod]
    public void QueryStoreCaptureModeAutoNotDetected()
    {
        var options = new TSqlModelOptions { QueryStoreCaptureMode = QueryStoreCaptureMode.Auto };
        using var test = new RuleTest(new List<Tuple<string, string>>(), options, SqlVersion);
        test.RunTest(QueryStoreCaptureModeAutoRule.RuleId, (result, _) =>
        {
            Assert.AreEqual(0, result.Problems.Count, "Expected 0 problems when QUERY_STORE_CAPTURE_MODE is AUTO");
        });
    }

    [TestMethod]
    public void QueryStoreCaptureModeAzureSqlIgnored()
    {
        var options = new TSqlModelOptions();
        using var test = new RuleTest(new List<Tuple<string, string>>(), options, SqlServerVersion.SqlAzure);
        test.RunTest(QueryStoreCaptureModeAutoRule.RuleId, (result, _) =>
        {
            Assert.AreEqual(0, result.Problems.Count, "Expected 0 problems for Azure SQL Database target");
        });
    }

    [TestMethod]
    public void QueryStoreCaptureModePre2016Ignored()
    {
        var options = new TSqlModelOptions();
        using var test = new RuleTest(new List<Tuple<string, string>>(), options, SqlServerVersion.Sql120);
        test.RunTest(QueryStoreCaptureModeAutoRule.RuleId, (result, _) =>
        {
            Assert.AreEqual(0, result.Problems.Count, "Expected 0 problems for SQL Server versions before 2016");
        });
    }
}
