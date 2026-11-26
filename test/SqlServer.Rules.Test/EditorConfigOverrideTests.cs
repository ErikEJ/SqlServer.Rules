using System;
using System.IO;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlServer.Rules.Tests;

[TestClass]
public class EditorConfigOverrideTests
{
    private const string Sql = "CREATE TABLE dbo.Sample (Email nvarchar(200) NULL, Something int CONSTRAINT Custom_Sample_Something DEFAULT (1));";
    private string directory = null!;
    private string configPath = null!;

    [TestInitialize]
    public void SetUp()
    {
        directory = Path.Combine(Path.GetTempPath(), "RuleEditorConfig-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        configPath = Path.Combine(directory, ".editorconfig");
        File.WriteAllText(Path.Combine(directory, "policy.json"), """
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
    public void ExplicitConfigAppliesOnlySqlSectionsToBothRules()
    {
        using var model = new TSqlModel(SqlServerVersion.Sql150, new TSqlModelOptions());
        model.AddObjects(Sql);
        var service = new CodeAnalysisServiceFactory().CreateAnalysisService(model.Version);
        var config = RuleEditorConfig.Load(configPath);
        foreach (var descriptor in service.GetRules())
        {
            if (descriptor.Rule is BaseSqlCodeAnalysisRule rule)
            {
                rule.EditorConfig = config;
            }
        }

        var result = service.Analyze(model);
        Assert.IsTrue(result.AnalysisSucceeded);
        Assert.AreEqual(0, result.AnalysisErrors.Count, string.Join(Environment.NewLine, result.AnalysisErrors.Select(error => error.Message)));
        var naming = result.Problems.Single(problem => problem.RuleId == "SqlServer.Rules.SRN0007");
        StringAssert.Contains(naming.ShortErrorMessage, "^Override_Sample_Something$", StringComparison.Ordinal);
        var sensitivity = result.Problems.Single(problem => problem.RuleId == "SqlServer.Rules.SRD0097");
        StringAssert.Contains(sensitivity.ShortErrorMessage, "potentially sensitive", StringComparison.Ordinal);
    }

    [TestMethod]
    public void ConfigRejectsMalformedAndMissingFiles()
    {
        File.WriteAllText(configPath, "[*.sql\nsqlserver_rules.srd0097.policy_file = policy.json\n");
        var invalid = Assert.Throws<FormatException>(() => RuleEditorConfig.Load(configPath));
        StringAssert.Contains(invalid.Message, "line 1", StringComparison.Ordinal);
        File.Delete(configPath);
        Assert.Throws<FileNotFoundException>(() => RuleEditorConfig.Load(configPath));
    }

    [TestMethod]
    public void LaterSectionsCanUnsetSettingsAndResolveExceptionsRelativeToConfig()
    {
        File.WriteAllText(Path.Combine(directory, "exceptions.json"), """
            {"exceptions":[{"column":"[dbo].[Sample].[Email]","reason":"Test exception","approvedBy":"Reviewer"}]}
            """);
        File.AppendAllText(configPath, "\n[*.sql]\nsqlserver_rules.srn0007.df_regex = unset\nsqlserver_rules.srd0097.exceptions_file = exceptions.json\n");
        using var model = new TSqlModel(SqlServerVersion.Sql150, new TSqlModelOptions());
        model.AddObjects(Sql);
        var service = new CodeAnalysisServiceFactory().CreateAnalysisService(model.Version);
        var config = RuleEditorConfig.Load(configPath);
        foreach (var descriptor in service.GetRules())
        {
            if (descriptor.Rule is BaseSqlCodeAnalysisRule rule)
            {
                rule.EditorConfig = config;
            }
        }

        var result = service.Analyze(model);
        Assert.IsTrue(result.AnalysisSucceeded);
        Assert.AreEqual(0, result.AnalysisErrors.Count, string.Join(Environment.NewLine, result.AnalysisErrors.Select(error => error.Message)));
        Assert.IsFalse(result.Problems.Any(problem => problem.RuleId == "SqlServer.Rules.SRD0097"));
        var naming = result.Problems.Single(problem => problem.RuleId == "SqlServer.Rules.SRN0007");
        StringAssert.Contains(naming.ShortErrorMessage, "^DF_Sample_Something$", StringComparison.Ordinal);
    }
}
