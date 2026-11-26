using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using ErikEJ.DacFX.TSQLAnalyzer;
using ErikEJ.DacFX.TSQLAnalyzer.Protocol;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlAnalyzerCli.Services;

namespace SqlServer.Rules.Tests.Docs;

[TestClass]
[TestCategory("Analyzer")]
public class EditorConfigOverrideTests
{
    private const string Sql = "CREATE TABLE dbo.Sample (Id int NOT NULL, Email nvarchar(200) NULL, Something int CONSTRAINT Custom_Sample_Something DEFAULT (1));";
    private string directory = null!;
    private string configPath = null!;
    private string sqlPath = null!;

    [TestInitialize]
    public void SetUp()
    {
        directory = Path.Combine(Path.GetTempPath(), "EditorConfigOverride-" + Guid.NewGuid().ToString("N"));
        var sourceDirectory = Path.Combine(directory, "source");
        var configDirectory = Path.Combine(directory, "config");
        Directory.CreateDirectory(sourceDirectory);
        Directory.CreateDirectory(configDirectory);
        sqlPath = Path.Combine(sourceDirectory, "Sample.sql");
        configPath = Path.Combine(configDirectory, ".editorconfig");
        File.WriteAllText(sqlPath, Sql);
        File.WriteAllText(Path.Combine(sourceDirectory, ".editorconfig"), "root = true\n[*.sql]\nsqlserver_rules.srn0007.df_regex = ^Source_{{tableName}}_{{columnName}}$\n");
        File.WriteAllText(Path.Combine(configDirectory, "policy.json"), """
            {"Version":"2.0","Labels":{"confidential":{"DisplayName":"Confidential","Rank":"Medium","Enabled":true}},"InformationTypes":{"contact":{"DisplayName":"Contact Info","Order":1,"RecommendedLabelId":"confidential","Enabled":true,"Keywords":[{"Pattern":"%email%","CanBeNumeric":false}]}}}
            """);
        File.WriteAllText(configPath, """
            root = true
            [*.sql]
            sqlserver_rules.srn0007.df_regex = ^Override_{{tableName}}_{{columnName}}$
            sqlserver_rules.srd0097.policy_file = policy.json
            [Sample.sql]
            sqlserver_rules.srn0007.df_regex = ^Ignored_{{tableName}}_{{columnName}}$
            sqlserver_rules.srd0097.policy_file = missing.json
            """);
    }

    [TestCleanup]
    public void CleanUp() => Directory.Delete(directory, recursive: true);

    [TestMethod]
    public void ExplicitConfigOverridesSourceAndOnlyUsesLiteralSqlSections()
    {
        var defaultResult = new AnalyzerFactory(new AnalyzerOptions { Scripts = [sqlPath] }).Analyze();
        var sourceNaming = defaultResult.Result!.Problems.Single(p => p.RuleId == "SqlServer.Rules.SRN0007");
        StringAssert.Contains(sourceNaming.ShortErrorMessage, "^Source_Sample_Something$", StringComparison.Ordinal);
        Assert.IsFalse(defaultResult.Result.Problems.Any(p => p.RuleId == "SqlServer.Rules.SRD0097"));

        var result = new AnalyzerFactory(new AnalyzerOptions { Scripts = [sqlPath], EditorConfigPath = configPath }).Analyze();
        Assert.IsTrue(result.Result!.AnalysisSucceeded);
        Assert.AreEqual(0, result.Result.AnalysisErrors.Count);
        var naming = result.Result.Problems.Single(p => p.RuleId == "SqlServer.Rules.SRN0007");
        StringAssert.Contains(naming.ShortErrorMessage, "^Override_Sample_Something$", StringComparison.Ordinal);
        Assert.AreEqual(1, result.Result.Problems.Count(p => p.RuleId == "SqlServer.Rules.SRD0097"));
    }

    [TestMethod]
    public void ExplicitConfigWorksForTextZipAndDacpac()
    {
        var text = new AnalyzerFactory(new AnalyzerOptions { Script = Sql, EditorConfigPath = configPath }).Analyze();
        Assert.IsTrue(text.Result!.Problems.Any(p => p.RuleId == "SqlServer.Rules.SRD0097"));
        Assert.IsTrue(text.Result.Problems.Any(p => p.RuleId == "SqlServer.Rules.SRN0007" && p.ShortErrorMessage.Contains("^Override_", StringComparison.Ordinal)));

        var zipPath = Path.Combine(directory, "sample.zip");
        ZipFile.CreateFromDirectory(Path.GetDirectoryName(sqlPath)!, zipPath);
        var zip = new AnalyzerFactory(new AnalyzerOptions { Scripts = [zipPath], EditorConfigPath = configPath }).Analyze();
        Assert.IsTrue(zip.Result!.Problems.Any(p => p.RuleId == "SqlServer.Rules.SRD0097"));
        Assert.IsTrue(zip.Result.Problems.Any(p => p.RuleId == "SqlServer.Rules.SRN0007" && p.ShortErrorMessage.Contains("^Override_", StringComparison.Ordinal)));

        var dacpac = Path.GetFullPath("../../../../../tools/SqlAnalyzerCli/testfiles/Chinook.dacpac", AppContext.BaseDirectory);
        var result = new AnalyzerFactory(new AnalyzerOptions { Scripts = [dacpac], EditorConfigPath = configPath }).Analyze();
        Assert.AreEqual(2, result.Result!.Problems.Count(p => p.RuleId == "SqlServer.Rules.SRD0097"));
    }

    [TestMethod]
    public void RelativeExceptionsAndUnsetSettingsAreRespected()
    {
        File.WriteAllText(Path.Combine(Path.GetDirectoryName(configPath)!, "exceptions.json"), """
            {"exceptions":[{"column":"[dbo].[Sample].[Email]","reason":"Test exception","approvedBy":"Reviewer"}]}
            """);
        File.AppendAllText(configPath, "\n[*.sql]\nsqlserver_rules.srd0097.exceptions_file = exceptions.json\nsqlserver_rules.srn0007.df_regex = unset\n");
        var result = new AnalyzerFactory(new AnalyzerOptions { Script = Sql, EditorConfigPath = configPath }).Analyze();
        Assert.IsTrue(result.Result!.AnalysisSucceeded);
        Assert.AreEqual(0, result.Result.AnalysisErrors.Count);
        Assert.IsFalse(result.Result.Problems.Any(p => p.RuleId == "SqlServer.Rules.SRD0097"));
        var naming = result.Result.Problems.Single(p => p.RuleId == "SqlServer.Rules.SRN0007");
        StringAssert.Contains(naming.ShortErrorMessage, "^DF_Sample_Something$", StringComparison.Ordinal);
    }

    [TestMethod]
    public void InvalidPolicyProducesRuleProblem()
    {
        File.WriteAllText(Path.Combine(Path.GetDirectoryName(configPath)!, "policy.json"), "{\"Version\":\"2.0\",\"Foo\":true,\"Labels\":{},\"InformationTypes\":{}}");
        var result = new AnalyzerFactory(new AnalyzerOptions { Script = Sql, EditorConfigPath = configPath }).Analyze();
        Assert.IsTrue(result.Result!.AnalysisSucceeded);
        var problem = result.Result.Problems.Single(p => p.RuleId == "SqlServer.Rules.SRD0097");
        StringAssert.Contains(problem.ShortErrorMessage, "unsupported field 'Foo'", StringComparison.Ordinal);
    }

    [TestMethod]
    public void ServerUsesConfigAndReloadsChangesBetweenRequests()
    {
        var request = new ServerRequest { Id = "1", Command = "analyze", Content = Sql, EditorConfigPath = configPath };
        var response = ServerMode.BuildAnalyzeResponse(request);
        Assert.AreEqual("success", response.Status, response.Error);
        Assert.AreEqual(true, response.EditorConfigApplied);
        Assert.IsNotNull(response.Problems);
        Assert.IsTrue(response.Problems.Any(p => p.Rule == "SqlServer.Rules.SRD0097"));
        Assert.IsTrue(response.Problems.Any(p => p.Rule == "SqlServer.Rules.SRN0007" && p.Message.Contains("^Override_", StringComparison.Ordinal)));

        File.WriteAllText(configPath, "[*.sql]\nsqlserver_rules.srn0007.df_regex = ^Custom_{{tableName}}_{{columnName}}$\n");
        response = ServerMode.BuildAnalyzeResponse(request);
        Assert.AreEqual("success", response.Status, response.Error);
        Assert.IsFalse(response.Problems!.Any(p => p.Rule == "SqlServer.Rules.SRD0097" || p.Rule == "SqlServer.Rules.SRN0007"));

        request.EditorConfigPath = null;
        response = ServerMode.BuildAnalyzeResponse(request);
        Assert.AreEqual("success", response.Status, response.Error);
        Assert.IsNull(response.EditorConfigApplied);
        Assert.IsNotNull(response.Problems);
        Assert.IsTrue(response.Problems.Any(p => p.Rule == "SqlServer.Rules.SRN0007" && p.Message.Contains("^DF_", StringComparison.Ordinal)));
        Assert.IsFalse(response.Problems.Any(p => p.Rule == "SqlServer.Rules.SRD0097"));
    }

    [TestMethod]
    public void ServerReportsMalformedAndMissingConfig()
    {
        var request = new ServerRequest { Id = "1", Command = "analyze", Content = Sql, EditorConfigPath = configPath };
        File.WriteAllText(configPath, "[*.sql\nsqlserver_rules.srd0097.policy_file = policy.json\n");
        var response = ServerMode.BuildAnalyzeResponse(request);
        Assert.AreEqual("error", response.Status);
        StringAssert.Contains(response.Error!, "Invalid EditorConfig syntax", StringComparison.Ordinal);

        request.Content = string.Empty;
        response = ServerMode.BuildAnalyzeResponse(request);
        Assert.AreEqual("error", response.Status);
        StringAssert.Contains(response.Error!, "Invalid EditorConfig syntax", StringComparison.Ordinal);

        File.WriteAllText(configPath, "[*.sql]\n");
        response = ServerMode.BuildAnalyzeResponse(request);
        Assert.AreEqual("success", response.Status, response.Error);
        Assert.AreEqual(true, response.EditorConfigApplied);

        File.Delete(configPath);
        response = ServerMode.BuildAnalyzeResponse(request);
        Assert.AreEqual("error", response.Status);
        StringAssert.Contains(response.Error!, configPath, StringComparison.Ordinal);
    }
}
