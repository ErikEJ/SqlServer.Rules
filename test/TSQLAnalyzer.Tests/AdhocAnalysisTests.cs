using System;
using System.Linq;
using ErikEJ.DacFX.TSQLAnalyzer;
using ErikEJ.DacFX.TSQLAnalyzer.Services;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlServer.Rules.Tests.Docs;

[TestClass]
[TestCategory("Analyzer")]
public class AdhocAnalysisTests
{
    private const string SelectStarRuleId = "SqlServer.Rules.SRD0006";

    [TestMethod]
    public void DesignRulesFireOnWrappedAdhocBatch()
    {
        // A plain ad-hoc query, not an object definition.
        var result = Analyze(new AnalyzerOptions
        {
            Script = "SELECT * FROM sys.objects;",
            SqlVersion = SqlServerVersion.Sql160,
        });

        // The SELECT * design rule fires even though there is no CREATE statement.
        Assert.IsNotEmpty(result.Result!.Problems, "Expected design problems on the ad-hoc batch.");
        Assert.IsTrue(
            result.Result.Problems.Any(p => p.RuleId == SelectStarRuleId),
            $"Expected {SelectStarRuleId} (Avoid SELECT *) to fire on the wrapped ad-hoc batch.");
    }

    [TestMethod]
    public void NamingAndSyntheticObjectNoiseIsSuppressed()
    {
        var result = Analyze(new AnalyzerOptions
        {
            Script = "SELECT * FROM sys.objects;",
            SqlVersion = SqlServerVersion.Sql160,
        });

        foreach (var problem in result.Result!.Problems)
        {
            Assert.IsFalse(
                problem.RuleId.Contains(".SRN", StringComparison.OrdinalIgnoreCase),
                $"Naming rule {problem.RuleId} should be suppressed for synthetic ad-hoc objects.");
            Assert.AreNotEqual(
                "SqlServer.Rules.SRP0005",
                problem.RuleId,
                "SET NOCOUNT ON (SRP0005) should be suppressed for synthetic ad-hoc objects.");
            Assert.IsFalse(
                (problem.SourceName ?? string.Empty).Contains(BatchWrapper.SyntheticObjectPrefix, StringComparison.Ordinal),
                "Problems must not reference the synthetic wrapper object name.");
        }
    }

    [TestMethod]
    public void ReportedLineNumbersMapToOriginalSource()
    {
        // The fixture has `SELECT *` on line 3 (after two comment lines).
        var result = Analyze(new AnalyzerOptions
        {
            Scripts = ["../../../../../sqlprojects/AdhocScripts/AdhocDml.sql"],
            SqlVersion = SqlServerVersion.Sql160,
        });

        var selectStar = result.Result!.Problems.FirstOrDefault(p => p.RuleId == SelectStarRuleId);

        Assert.IsNotNull(selectStar, $"Expected {SelectStarRuleId} to fire on the ad-hoc file.");
        Assert.AreEqual(
            3,
            selectStar.StartLine,
            "Inline wrapping must keep the reported line number aligned with the original source line.");
    }

    [TestMethod]
    public void ReportedColumnNumbersMapToOriginalSource()
    {
        // The fixture has a DELETE on line 4 (after USE / GO / blank line).
        // The wrapper prefix is inserted on the same line as DELETE, so without the fix the
        // reported column would be 64 (= 1 + 63-char prefix) instead of 1.
        var result = Analyze(new AnalyzerOptions
        {
            Scripts = ["../../../../../sqlprojects/AdhocScripts/AdhocBatchStartColumn.sql"],
            SqlVersion = SqlServerVersion.Sql160,
        });

        var deleteWithoutWhere = result.Result!.Problems.FirstOrDefault(p => p.RuleId == "SqlServer.Rules.SRD0017");

        Assert.IsNotNull(deleteWithoutWhere, "Expected SRD0017 (DELETE without WHERE) to fire on the ad-hoc file.");
        Assert.AreEqual(
            4,
            deleteWithoutWhere.StartLine,
            "The DELETE statement should be on line 4.");

        var adjustedColumn = result.GetAdjustedColumn(
            deleteWithoutWhere.StartLine,
            deleteWithoutWhere.StartColumn,
            deleteWithoutWhere.SourceName);

        Assert.AreEqual(
            1,
            adjustedColumn,
            "Inline wrapping must not shift the reported column; the DELETE is at column 1 in the original source.");
    }

    [TestMethod]
    public void EveryGoSeparatedBatchIsAnalyzed()
    {
        var result = Analyze(new AnalyzerOptions
        {
            Scripts = ["../../../../../sqlprojects/AdhocScripts/AdhocMultiBatch.sql"],
            SqlVersion = SqlServerVersion.Sql160,
        });

        // Both GO-separated SELECT * batches should be flagged.
        Assert.AreEqual(
            2,
            result.Result!.Problems.Count(p => p.RuleId == SelectStarRuleId),
            "Each wrapped ad-hoc batch should be analyzed independently.");
    }

    [TestMethod]
    public void RealObjectAndAdhocBatchCoexistWithoutError()
    {
        // A real CREATE TABLE batch mixed with an ad-hoc SELECT * batch must not abort analysis.
        var result = Analyze(new AnalyzerOptions
        {
            Scripts = ["../../../../../sqlprojects/AdhocScripts/AdhocMixed.sql"],
            SqlVersion = SqlServerVersion.Sql160,
        });

        Assert.IsTrue(result.Result!.AnalysisSucceeded, "Analysis should succeed for a mixed object/ad-hoc file.");
        Assert.IsEmpty(result.ModelErrors, "Un-wrappable batches must not produce fatal model errors.");
        Assert.IsTrue(
            result.Result.Problems.Any(p => p.RuleId == SelectStarRuleId),
            "The ad-hoc SELECT * batch should still be analyzed alongside the real object.");
    }

    private static AnalyzerResult Analyze(AnalyzerOptions options)
    {
        var analysis = new AnalyzerFactory(options).Analyze();

        Assert.IsNotNull(analysis);
        Assert.IsNotNull(analysis.Result);

        return analysis;
    }
}
