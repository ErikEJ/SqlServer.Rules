using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using ErikEJ.DacFX.TSQLAnalyzer;
using ErikEJ.DacFX.TSQLAnalyzer.Extensions;
using ErikEJ.DacFX.TSQLAnalyzer.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlServer.Rules.Tests.Docs;

[TestClass]
[TestCategory("Analyzer")]
public class SarifOutputTests
{
    private const string SelectStarRuleId = "SqlServer.Rules.SRD0006";
    private const string Procedure = "CREATE PROCEDURE dbo.GetObjects\r\nAS\r\nSELECT * FROM sys.objects;\r\n";
    private string directory = null!;
    private string sqlPath = null!;
    private string sarifPath = null!;

    [TestInitialize]
    public void SetUp()
    {
        directory = Path.Combine(Path.GetTempPath(), "Sarif-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        sqlPath = Path.Combine(directory, "GetObjects.sql");
        sarifPath = Path.Combine(directory, "result.sarif");
        File.WriteAllText(sqlPath, Procedure, new UTF8Encoding(true));
    }

    [TestCleanup]
    public void CleanUp() => Directory.Delete(directory, recursive: true);

    [TestMethod]
    public void WritesResultsWithRulesAndRegionsForFiles()
    {
        var analysis = Analyze(new AnalyzerOptions { Scripts = [sqlPath] });

        Assert.AreEqual((byte)'{', File.ReadAllBytes(sarifPath)[0], "SARIF must not start with a byte order mark.");

        using var sarif = JsonDocument.Parse(File.ReadAllText(sarifPath));
        Assert.AreEqual("2.1.0", sarif.RootElement.GetProperty("version").GetString());

        var run = sarif.RootElement.GetProperty("runs")[0];
        var rules = run.GetProperty("tool").GetProperty("driver").GetProperty("rules");
        var results = run.GetProperty("results");

        Assert.AreEqual(analysis.Problems.Count, results.GetArrayLength());
        Assert.AreEqual(analysis.Problems.Select(problem => problem.RuleId).Distinct().Count(), rules.GetArrayLength());

        foreach (var result in results.EnumerateArray())
        {
            var rule = rules[result.GetProperty("ruleIndex").GetInt32()];
            Assert.AreEqual(result.GetProperty("ruleId").GetString(), rule.GetProperty("id").GetString());
        }

        var selectStar = results.EnumerateArray().Single(result => result.GetProperty("ruleId").GetString() == SelectStarRuleId);
        Assert.AreEqual("warning", selectStar.GetProperty("level").GetString());
        Assert.IsFalse(selectStar.GetProperty("message").GetProperty("text").GetString()!.Contains("http", StringComparison.Ordinal));

        var location = selectStar.GetProperty("locations")[0].GetProperty("physicalLocation");
        Assert.AreEqual(new Uri(sqlPath).AbsoluteUri, location.GetProperty("artifactLocation").GetProperty("uri").GetString());

        var region = location.GetProperty("region");
        Assert.AreEqual(3, region.GetProperty("startLine").GetInt32());
        Assert.AreEqual(8, region.GetProperty("startColumn").GetInt32());
        Assert.AreEqual(3, region.GetProperty("endLine").GetInt32());
        Assert.AreEqual(9, region.GetProperty("endColumn").GetInt32());

        var selectStarRule = rules.EnumerateArray().Single(rule => rule.GetProperty("id").GetString() == SelectStarRuleId);
        StringAssert.EndsWith(selectStarRule.GetProperty("helpUri").GetString(), "/SRD0006.md", StringComparison.Ordinal);
    }

    [TestMethod]
    [DataRow("ALTER PROCEDURE dbo.GetObjects AS SELECT * FROM sys.objects;", true)]
    [DataRow("CREATE OR ALTER PROCEDURE dbo.GetObjects AS SELECT * FROM sys.objects;", false)]
    [DataRow("SELECT * FROM sys.objects;", true)]
    public void RegionsUseOriginalSourceColumnsAfterPreprocessing(string sql, bool shiftsColumns)
    {
        File.WriteAllText(sqlPath, sql, new UTF8Encoding(true));
        var analysis = Analyze(new AnalyzerOptions { Scripts = [sqlPath] });
        var problem = analysis.Problems.Single(problem => problem.RuleId == SelectStarRuleId);
        var expectedStartColumn = sql.IndexOf('*', StringComparison.Ordinal) + 1;
        var expectedEndColumn = expectedStartColumn + 1;

        Assert.AreEqual(shiftsColumns, expectedEndColumn != problem.GetEndPosition().EndColumn);
        Assert.AreEqual(expectedEndColumn, problem.GetEndPosition(analysis).EndColumn);

        using var sarif = JsonDocument.Parse(File.ReadAllText(sarifPath));
        var finding = sarif.RootElement.GetProperty("runs")[0].GetProperty("results").EnumerateArray()
            .Single(result => result.GetProperty("ruleId").GetString() == SelectStarRuleId);
        var region = finding.GetProperty("locations")[0].GetProperty("physicalLocation").GetProperty("region");

        Assert.AreEqual(1, region.GetProperty("startLine").GetInt32());
        Assert.AreEqual(expectedStartColumn, region.GetProperty("startColumn").GetInt32());
        Assert.AreEqual(1, region.GetProperty("endLine").GetInt32());
        Assert.AreEqual(expectedEndColumn, region.GetProperty("endColumn").GetInt32());
    }

    [TestMethod]
    public void RulesPromotedToErrorsHaveErrorLevel()
    {
        Analyze(new AnalyzerOptions { Scripts = [sqlPath], Rules = "Rules:+!SqlServer.Rules.SRD*" });

        using var sarif = JsonDocument.Parse(File.ReadAllText(sarifPath));
        var results = sarif.RootElement.GetProperty("runs")[0].GetProperty("results").EnumerateArray().ToList();

        Assert.AreEqual("error", results.Single(result => result.GetProperty("ruleId").GetString() == SelectStarRuleId).GetProperty("level").GetString());
        Assert.IsTrue(results
            .Where(result => !result.GetProperty("ruleId").GetString()!.StartsWith("SqlServer.Rules.SRD", StringComparison.Ordinal))
            .All(result => result.GetProperty("level").GetString() == "warning"));
    }

    [TestMethod]
    public void BaselinedProblemsAreOmitted()
    {
        var baselinePath = Path.Combine(directory, "baseline.json");
        new AnalyzerFactory(new AnalyzerOptions { Scripts = [sqlPath], BaselinePath = baselinePath, UpdateBaseline = true }).Analyze();

        var analysis = Analyze(new AnalyzerOptions { Scripts = [sqlPath], BaselinePath = baselinePath });

        Assert.IsNotEmpty(analysis.Result!.Problems);
        using var sarif = JsonDocument.Parse(File.ReadAllText(sarifPath));
        Assert.AreEqual(0, sarif.RootElement.GetProperty("runs")[0].GetProperty("results").GetArrayLength());
    }

    [TestMethod]
    public void ScriptTextProblemsHaveNoLocation()
    {
        var analysis = Analyze(new AnalyzerOptions { Script = Procedure });

        using var sarif = JsonDocument.Parse(File.ReadAllText(sarifPath));
        var results = sarif.RootElement.GetProperty("runs")[0].GetProperty("results");

        Assert.AreEqual(analysis.Problems.Count, results.GetArrayLength());
        Assert.IsTrue(results.EnumerateArray().All(result => result.GetProperty("locations").GetArrayLength() == 0));
    }

    [TestMethod]
    public void DacpacProblemsAreLocatedByObjectName()
    {
        var dacpac = Path.GetFullPath("../../../../../tools/SqlAnalyzerCli/testfiles/Chinook.dacpac", AppContext.BaseDirectory);

        Analyze(new AnalyzerOptions { Scripts = [dacpac] });

        using var sarif = JsonDocument.Parse(File.ReadAllText(sarifPath));
        var locations = sarif.RootElement.GetProperty("runs")[0].GetProperty("results").EnumerateArray()
            .SelectMany(result => result.GetProperty("locations").EnumerateArray())
            .ToList();

        Assert.IsNotEmpty(locations);
        Assert.IsFalse(locations.Any(location => location.TryGetProperty("physicalLocation", out _)));
        Assert.IsTrue(locations.Any(location => location.GetProperty("logicalLocations")[0].GetProperty("fullyQualifiedName").GetString() == "[dbo].[Album]"));
    }

    [TestMethod]
    public void ArtifactUriIsRelativeBelowTheBaseDirectoryAndAbsoluteOtherwise()
    {
        var nested = Path.Combine(directory, "My Procs", "a#b.sql");

        Assert.AreEqual("My%20Procs/a%23b.sql", SarifWriter.GetArtifactUri(nested, directory));
        Assert.AreEqual(new Uri(sqlPath).AbsoluteUri, SarifWriter.GetArtifactUri(sqlPath, Path.Combine(directory, "My Procs")));
    }

    private AnalyzerResult Analyze(AnalyzerOptions options)
    {
        options.OutputFile = new FileInfo(sarifPath);

        var analysis = new AnalyzerFactory(options).Analyze();

        Assert.IsNotNull(analysis.Result);
        Assert.IsTrue(File.Exists(sarifPath), "Expected a SARIF file to be written.");

        return analysis;
    }
}
