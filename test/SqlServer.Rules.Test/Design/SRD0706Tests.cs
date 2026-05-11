using System;
using System.Collections.Generic;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.Rules.Design;
using SqlServer.Rules.Tests.Utils;

namespace SqlServer.Rules.Tests.Design;

[TestClass]
[TestCategory("Design")]
public class SRD0706Tests : TestCasesBase
{
    [TestMethod]
    public void AutoShrinkOnDetected()
    {
        var options = new TSqlModelOptions { AutoShrink = true };
        using var test = new RuleTest(new List<Tuple<string, string>>(), options, SqlVersion);
        test.RunTest(AutoShrinkOffRule.RuleId, (result, _) =>
        {
            Assert.AreEqual(1, result.Problems.Count, "Expected 1 problem when AUTO_SHRINK is ON");
            Assert.IsTrue(result.Problems[0].Description.Contains(AutoShrinkOffRule.Message, StringComparison.Ordinal));
        });
    }

    [TestMethod]
    public void AutoShrinkOffNotDetected()
    {
        var options = new TSqlModelOptions { AutoShrink = false };
        using var test = new RuleTest(new List<Tuple<string, string>>(), options, SqlVersion);
        test.RunTest(AutoShrinkOffRule.RuleId, (result, _) =>
        {
            Assert.AreEqual(0, result.Problems.Count, "Expected 0 problems when AUTO_SHRINK is OFF");
        });
    }

    [TestMethod]
    public void AutoShrinkAzureSqlIgnored()
    {
        var options = new TSqlModelOptions { AutoShrink = true };
        using var test = new RuleTest(new List<Tuple<string, string>>(), options, SqlServerVersion.SqlAzure);
        test.RunTest(AutoShrinkOffRule.RuleId, (result, _) =>
        {
            Assert.AreEqual(0, result.Problems.Count, "Expected 0 problems for Azure SQL Database target");
        });
    }
}
