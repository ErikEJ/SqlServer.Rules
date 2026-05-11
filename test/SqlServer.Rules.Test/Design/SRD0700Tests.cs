using System;
using System.Collections.Generic;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.Rules.Design;
using SqlServer.Rules.Tests.Utils;

namespace SqlServer.Rules.Tests.Design;

[TestClass]
[TestCategory("Design")]
public class SRD0700Tests : TestCasesBase
{
    [TestMethod]
    public void PageVerifyNotChecksumDetected()
    {
        // Default TSqlModelOptions has PageVerifyMode = None, which should trigger the rule
        var options = new TSqlModelOptions();
        using var test = new RuleTest(new List<Tuple<string, string>>(), options, SqlVersion);
        test.RunTest(PageVerifyChecksumRule.RuleId, (result, _) =>
        {
            Assert.AreEqual(1, result.Problems.Count, "Expected 1 problem when PAGE_VERIFY is not CHECKSUM");
            Assert.IsTrue(result.Problems[0].Description.Contains(PageVerifyChecksumRule.Message, StringComparison.Ordinal));
        });
    }

    [TestMethod]
    public void PageVerifyChecksumNotDetected()
    {
        var options = new TSqlModelOptions { PageVerifyMode = PageVerifyMode.Checksum };
        using var test = new RuleTest(new List<Tuple<string, string>>(), options, SqlVersion);
        test.RunTest(PageVerifyChecksumRule.RuleId, (result, _) =>
        {
            Assert.AreEqual(0, result.Problems.Count, "Expected 0 problems when PAGE_VERIFY is CHECKSUM");
        });
    }

    [TestMethod]
    public void PageVerifyAzureSqlIgnored()
    {
        // Azure SQL Database manages PAGE_VERIFY internally; the rule should not fire for Azure targets
        var options = new TSqlModelOptions();
        using var test = new RuleTest(new List<Tuple<string, string>>(), options, SqlServerVersion.SqlAzure);
        test.RunTest(PageVerifyChecksumRule.RuleId, (result, _) =>
        {
            Assert.AreEqual(0, result.Problems.Count, "Expected 0 problems for Azure SQL Database target");
        });
    }
}
