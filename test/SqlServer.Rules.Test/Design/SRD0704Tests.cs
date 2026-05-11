using System;
using System.Collections.Generic;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.Rules.Design;
using SqlServer.Rules.Tests.Utils;

namespace SqlServer.Rules.Tests.Design;

[TestClass]
[TestCategory("Design")]
public class SRD0704Tests : TestCasesBase
{
    [TestMethod]
    public void TargetRecoveryTimePeriodZeroDetected()
    {
        // Default TSqlModelOptions has TargetRecoveryTimePeriod = 0, which should trigger the rule
        var options = new TSqlModelOptions();
        using var test = new RuleTest(new List<Tuple<string, string>>(), options, SqlVersion);
        test.RunTest(TargetRecoveryTimePeriodRule.RuleId, (result, _) =>
        {
            Assert.AreEqual(1, result.Problems.Count, "Expected 1 problem when TargetRecoveryTimePeriod is 0");
            Assert.IsTrue(result.Problems[0].Description.Contains(TargetRecoveryTimePeriodRule.Message, StringComparison.Ordinal));
        });
    }

    [TestMethod]
    public void TargetRecoveryTimePeriodPositiveNotDetected()
    {
        var options = new TSqlModelOptions { TargetRecoveryTimePeriod = 60 };
        using var test = new RuleTest(new List<Tuple<string, string>>(), options, SqlVersion);
        test.RunTest(TargetRecoveryTimePeriodRule.RuleId, (result, _) =>
        {
            Assert.AreEqual(0, result.Problems.Count, "Expected 0 problems when TargetRecoveryTimePeriod is greater than 0");
        });
    }

    [TestMethod]
    public void TargetRecoveryTimePeriodAzureSqlIgnored()
    {
        // Azure SQL manages TARGET_RECOVERY_TIME internally; the rule should not fire for Azure targets
        var options = new TSqlModelOptions();
        using var test = new RuleTest(new List<Tuple<string, string>>(), options, SqlServerVersion.SqlAzure);
        test.RunTest(TargetRecoveryTimePeriodRule.RuleId, (result, _) =>
        {
            Assert.AreEqual(0, result.Problems.Count, "Expected 0 problems for Azure SQL Database target");
        });
    }
}
