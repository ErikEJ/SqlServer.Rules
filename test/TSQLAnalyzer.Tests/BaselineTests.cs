using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using ErikEJ.DacFX.TSQLAnalyzer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlServer.Rules.Tests.Docs;

[TestClass]
[TestCategory("Analyzer")]
public class BaselineTests
{
    private const string SelectStarRuleId = "SqlServer.Rules.SRD0006";
    private const string NoCountRuleId = "SqlServer.Rules.SRP0005";
    private const string Procedure = "CREATE PROCEDURE dbo.GetObjects\r\nAS\r\nSELECT * FROM sys.objects;\r\n";
    private string directory = null!;
    private string sqlPath = null!;
    private string baselinePath = null!;

    [TestInitialize]
    public void SetUp()
    {
        directory = Path.Combine(Path.GetTempPath(), "Baseline-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(directory, "src"));
        sqlPath = Path.Combine(directory, "src", "GetObjects.sql");
        baselinePath = Path.Combine(directory, "baseline.json");
        WriteSql(sqlPath, Procedure);
    }

    [TestCleanup]
    public void CleanUp() => Directory.Delete(directory, recursive: true);

    [TestMethod]
    public void UpdateBaselineAcceptsTheProblemsFound()
    {
        var update = Analyze(updateBaseline: true);

        Assert.IsNotEmpty(update.Result!.Problems);
        Assert.IsEmpty(update.Problems);
        Assert.AreEqual(update.Result.Problems.Count, update.BaselinedProblemCount);

        using var baseline = JsonDocument.Parse(File.ReadAllText(baselinePath));
        Assert.AreEqual(1, baseline.RootElement.GetProperty("version").GetInt32());
        var rules = baseline.RootElement.GetProperty("problems").GetProperty("src/GetObjects.sql");
        Assert.AreEqual(1, rules.GetProperty(SelectStarRuleId).GetInt32());
        Assert.AreEqual(update.Result.Problems.Count, rules.EnumerateObject().Sum(rule => rule.Value.GetInt32()));

        var result = Analyze();

        Assert.IsEmpty(result.Problems);
        Assert.AreEqual(result.Result!.Problems.Count, result.BaselinedProblemCount);
    }

    [TestMethod]
    public void CaseDistinctSourcesDoNotShareAcceptedCounts()
    {
        File.WriteAllText(baselinePath, """
            {
              "version": 1,
              "problems": {
                "src/GetObjects.sql": { "sqlserver.rules.srd0006": 0 },
                "src/getobjects.sql": { "SqlServer.Rules.SRD0006": 100 }
              }
            }
            """);

        var result = Analyze();

        Assert.AreEqual(1, result.Problems.Count(problem => problem.RuleId == SelectStarRuleId));
    }

    [TestMethod]
    public void WithoutBaselineAllProblemsAreReported()
    {
        var result = new AnalyzerFactory(new AnalyzerOptions { Scripts = [sqlPath] }).Analyze();

        Assert.IsNotEmpty(result.Problems);
        CollectionAssert.AreEqual(result.Result!.Problems.ToList(), result.Problems.ToList());
        Assert.AreEqual(0, result.BaselinedProblemCount);
    }

    [TestMethod]
    public void BaselinedProblemsStayCoveredWhenLinesShift()
    {
        Analyze(updateBaseline: true);
        WriteSql(sqlPath, "-- Added header\r\n\r\n" + Procedure);

        var result = Analyze();

        Assert.IsEmpty(result.Problems);
    }

    [TestMethod]
    public void AdditionalProblemReportsAllProblemsOfThatRuleInTheFile()
    {
        Analyze(updateBaseline: true);
        WriteSql(sqlPath, Procedure + "SELECT * FROM sys.tables;\r\n");

        var result = Analyze();

        Assert.AreEqual(2, result.Problems.Count(problem => problem.RuleId == SelectStarRuleId));
        Assert.AreEqual(1, result.Result!.Problems.Count(problem => problem.RuleId == NoCountRuleId));
        Assert.IsFalse(result.Problems.Any(problem => problem.RuleId == NoCountRuleId));
        Assert.AreEqual(result.Result.Problems.Count - result.Problems.Count, result.BaselinedProblemCount);
    }

    [TestMethod]
    public void ProblemsInNewFilesAreReported()
    {
        Analyze(updateBaseline: true);
        var newPath = Path.Combine(directory, "src", "GetTables.sql");
        WriteSql(newPath, "CREATE PROCEDURE dbo.GetTables\r\nAS\r\nSELECT * FROM sys.tables;\r\n");

        var result = Analyze();

        Assert.IsNotEmpty(result.Problems);
        Assert.IsTrue(result.Problems.All(problem => problem.SourceName == newPath));
    }

    [TestMethod]
    public void BaselineMatchesFilesRelativeToItsOwnLocation()
    {
        Analyze(updateBaseline: true);
        var moved = directory + "-moved";
        Directory.Move(directory, moved);
        directory = moved;

        var result = new AnalyzerFactory(new AnalyzerOptions
        {
            Scripts = [Path.Combine(moved, "src")],
            BaselinePath = Path.Combine(moved, "baseline.json"),
        }).Analyze();

        Assert.IsNotEmpty(result.Result!.Problems);
        Assert.IsEmpty(result.Problems);
    }

    [TestMethod]
    public void JsonOutputContainsOnlyProblemsNotInBaseline()
    {
        Analyze(updateBaseline: true);
        WriteSql(sqlPath, Procedure + "SELECT * FROM sys.tables;\r\n");
        var outputPath = Path.Combine(directory, "result.json");

        var result = new AnalyzerFactory(new AnalyzerOptions
        {
            Scripts = [Path.Combine(directory, "src")],
            BaselinePath = baselinePath,
            OutputFile = new FileInfo(outputPath),
        }).Analyze();

        using var output = JsonDocument.Parse(File.ReadAllText(outputPath));
        var problems = output.RootElement.GetProperty("Problems");
        Assert.AreEqual(result.Problems.Count, problems.GetArrayLength());
        Assert.IsFalse(problems.EnumerateArray().Any(problem => problem.GetProperty("Rule").GetString() == NoCountRuleId));
    }

    [TestMethod]
    public void InvalidBaselineOptionsAreRejected()
    {
        var missing = Assert.Throws<ArgumentException>(() => Analyze());
        StringAssert.Contains(missing.Message, "was not found", StringComparison.Ordinal);

        Assert.Throws<ArgumentException>(() => new AnalyzerFactory(new AnalyzerOptions { Scripts = [sqlPath], UpdateBaseline = true }).Analyze());

        var xml = Assert.Throws<ArgumentException>(() => new AnalyzerFactory(new AnalyzerOptions
        {
            Scripts = [sqlPath],
            BaselinePath = baselinePath,
            UpdateBaseline = true,
            OutputFile = new FileInfo(Path.Combine(directory, "result.xml")),
        }).Analyze());
        StringAssert.Contains(xml.Message, "xml", StringComparison.Ordinal);
        Assert.IsFalse(File.Exists(baselinePath));

        File.WriteAllText(baselinePath, "{ \"version\": 1, \"problems\": [");
        var malformed = Assert.Throws<ArgumentException>(() => Analyze());
        StringAssert.Contains(malformed.Message, "Unable to load baseline file", StringComparison.Ordinal);

        File.WriteAllText(baselinePath, "{ \"version\": 2, \"problems\": {} }");
        var unsupported = Assert.Throws<ArgumentException>(() => Analyze());
        StringAssert.Contains(unsupported.Message, "version 1", StringComparison.Ordinal);
    }

    [TestMethod]
    public void ZipInputIsRejected()
    {
        var zipPath = Path.Combine(directory, "src.zip");
        ZipFile.CreateFromDirectory(Path.Combine(directory, "src"), zipPath);

        var zip = Assert.Throws<ArgumentException>(() => new AnalyzerFactory(new AnalyzerOptions
        {
            Scripts = [zipPath],
            BaselinePath = baselinePath,
            UpdateBaseline = true,
        }).Analyze());

        StringAssert.Contains(zip.Message, ".zip", StringComparison.Ordinal);
        Assert.IsFalse(File.Exists(baselinePath));
    }

    private static void WriteSql(string path, string sql)
        => File.WriteAllText(path, sql, new UTF8Encoding(true));

    private AnalyzerResult Analyze(bool updateBaseline = false)
        => new AnalyzerFactory(new AnalyzerOptions
        {
            Scripts = [Path.Combine(directory, "src")],
            BaselinePath = baselinePath,
            UpdateBaseline = updateBaseline,
        }).Analyze();
}
