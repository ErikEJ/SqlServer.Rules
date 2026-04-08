using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlServer.Rules.Tests.Naming;

[TestClass]
public class SRN0007EditorConfigTests
{
    private const string RuleId = "SqlServer.Rules.SRN0007";

    [TestMethod]
    public void DefaultRuleFlagsCustomDfPrefix()
    {
        const string sql = """
CREATE TABLE [dbo].[TableOne]
(
    [ColumnOne] INT CONSTRAINT [DEF_TableOne_ColumnOne] DEFAULT (1)
);
""";

        var problems = AnalyzeRuleProblems(sql, null);
        Assert.AreEqual(1, problems.Count);
    }

    [TestMethod]
    public void EditorConfigCanOverrideDfRegexWithSchemaToken()
    {
        const string sql = """
CREATE TABLE [dbo].[TableOne]
(
    [ColumnOne] INT CONSTRAINT [DEF_dbo_TableOne_ColumnOne] DEFAULT (1)
);
""";
        const string editorConfig = """
root = true

[*.sql]
sqlserver_rules.srn0007.df_regex = ^DEF_{{schemaName}}_{{tableName}}_{{columnName}}$
""";

        var problems = AnalyzeRuleProblems(sql, editorConfig);
        Assert.AreEqual(0, problems.Count);
    }

    [TestMethod]
    public void EditorConfigCanUseForeignSchemaTokens()
    {
        const string sql = """
CREATE TABLE [sales].[TableTwo]
(
    [Id] INT NOT NULL CONSTRAINT [PK_TableTwo] PRIMARY KEY
);

GO

CREATE TABLE [dbo].[TableOne]
(
    [Id] INT NOT NULL CONSTRAINT [PK_TableOne] PRIMARY KEY,
    [TableTwoId] INT NULL,
    CONSTRAINT [FK_dbo_TableOne_sales_TableTwo] FOREIGN KEY ([TableTwoId]) REFERENCES [sales].[TableTwo]([Id])
);
""";
        const string editorConfig = """
root = true

[*.sql]
sqlserver_rules.srn0007.fk_regex = ^FK_{{schemaName}}_{{tableName}}_{{foreignSchemaName}}_{{foreignTableName}}$
""";

        var problems = AnalyzeRuleProblems(sql, editorConfig);
        Assert.AreEqual(0, problems.Count);
    }

    private static List<SqlRuleProblem> AnalyzeRuleProblems(string sql, string editorConfig)
    {
        var folderPath = Path.Combine(Path.GetTempPath(), "SRN0007EditorConfigTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folderPath);
        var sqlPath = Path.Combine(folderPath, "test.sql");

        try
        {
            File.WriteAllText(sqlPath, sql);
            if (!string.IsNullOrWhiteSpace(editorConfig))
            {
                File.WriteAllText(Path.Combine(folderPath, ".editorconfig"), editorConfig);
            }

            using var model = new TSqlModel(SqlServerVersion.Sql150, null);
            model.AddOrUpdateObjects(sql, sqlPath, new TSqlObjectOptions());

            var ruleSettings = new CodeAnalysisRuleSettings
            {
                new RuleConfiguration(RuleId),
            };
            ruleSettings.DisableRulesNotInSettings = true;

            var service = new CodeAnalysisServiceFactory().CreateAnalysisService(model.Version, new CodeAnalysisServiceSettings
            {
                RuleSettings = ruleSettings,
            });

            var ruleLoadErrors = service.GetRuleLoadErrors();
            Assert.AreEqual(0, ruleLoadErrors.Count, string.Join(Environment.NewLine, ruleLoadErrors.Select(e => e.Message)));
            Assert.IsTrue(
                service.GetRules().Any(rule => rule.RuleId.Equals(RuleId, StringComparison.OrdinalIgnoreCase)),
                "Expected rule '{0}' not found by the service",
                RuleId);

            var result = service.Analyze(model);
            return result.Problems
                .Where(p => p.RuleId.Equals(RuleId, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
        finally
        {
            Directory.Delete(folderPath, recursive: true);
        }
    }
}
