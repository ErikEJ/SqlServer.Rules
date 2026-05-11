using System;
using System.Collections.Generic;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.Rules.Design;
using SqlServer.Rules.Tests.Utils;

namespace SqlServer.Rules.Tests.Design;

[TestClass]
[TestCategory("Design")]
public class SRD0705Tests : TestCasesBase
{
    [TestMethod]
    public void AutoCloseEnabledDetected()
    {
        var options = new TSqlModelOptions { AutoClose = true };
        using var test = new RuleTest(new List<Tuple<string, string>>(), options, SqlVersion);
        test.RunTest(AutoCloseOffRule.RuleId, (result, _) =>
        {
            Assert.AreEqual(1, result.Problems.Count, "Expected 1 problem when AUTO_CLOSE is enabled");
            Assert.IsTrue(result.Problems[0].Description.Contains(AutoCloseOffRule.Message, StringComparison.Ordinal));
        });
    }

    [TestMethod]
    public void AutoCloseFalseNotDetected()
    {
        var options = new TSqlModelOptions { AutoClose = false };
        using var test = new RuleTest(new List<Tuple<string, string>>(), options, SqlVersion);
        test.RunTest(AutoCloseOffRule.RuleId, (result, _) =>
        {
            Assert.AreEqual(0, result.Problems.Count, "Expected 0 problems when AUTO_CLOSE is disabled");
        });
    }

    [TestMethod]
    public void AutoCloseAzureSqlIgnored()
    {
        var options = new TSqlModelOptions { AutoClose = true };
        using var test = new RuleTest(new List<Tuple<string, string>>(), options, SqlServerVersion.SqlAzure);
        test.RunTest(AutoCloseOffRule.RuleId, (result, _) =>
        {
            Assert.AreEqual(0, result.Problems.Count, "Expected 0 problems for Azure SQL Database target");
        });
    }
}
