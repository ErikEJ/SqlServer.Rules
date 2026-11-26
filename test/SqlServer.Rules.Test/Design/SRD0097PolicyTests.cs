using System;
using System.IO;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.Rules.Design;

namespace SqlServer.Rules.Tests.Design;

[TestClass]
public class SRD0097PolicyTests
{
    [TestMethod]
    public void OptInPolicyFindingsRespectProtectionExclusionsAndClassification()
    {
        using var model = new TSqlModel(SqlServerVersion.Sql150, new TSqlModelOptions());
        model.AddObjects("CREATE TYPE dbo.PhoneAlias FROM int NOT NULL;");
        model.AddObjects("""
            CREATE TABLE dbo.PolicyTest
            (
                Email nvarchar(200) NULL,
                EmailAddress nvarchar(200) NULL,
                MaskedEmail nvarchar(200) MASKED WITH (FUNCTION = 'email()') NULL,
                PhoneNumber int NULL,
                PhoneAlias dbo.PhoneAlias NULL,
                Pin int NULL,
                PolicyExcludedEmail nvarchar(200) NULL,
                [Policy.Excluded] nvarchar(200) NULL,
                [Odd]]Name] nvarchar(200) NULL,
                ClassifiedEmail nvarchar(200) NULL,
                DisabledColumn nvarchar(200) NULL,
                ExpiredEmail nvarchar(200) NULL,
                TripAddress nvarchar(200) NULL,
                ShipAddress nvarchar(200) NULL,
                ZipCode nvarchar(200) NULL
            );
            """);
        model.AddObjects("""
            ADD SENSITIVITY CLASSIFICATION TO [dbo].[PolicyTest].[ClassifiedEmail]
                WITH (LABEL = 'Confidential', INFORMATION_TYPE = 'Contact Info', RANK = HIGH);
            """);

        var service = new CodeAnalysisServiceFactory().CreateAnalysisService(model.Version);
        var rule = service.GetRules()
            .Single(descriptor => descriptor.RuleId == UnprotectedSensitiveColumnsRule.RuleId)
            .Rule as UnprotectedSensitiveColumnsRule;
        Assert.IsNotNull(rule);
        rule.PolicyFile = Path.Combine(AppContext.BaseDirectory, "TestScripts", "SensitivityPolicy.json");
        rule.ExceptionsFile = Path.Combine(AppContext.BaseDirectory, "TestScripts", "SensitivityExceptions.json");

        var result = service.Analyze(model);
        var problems = result.Problems.Where(problem => problem.RuleId == UnprotectedSensitiveColumnsRule.RuleId).ToList();

        Assert.IsTrue(result.AnalysisSucceeded, "SRD0097 policy analysis should succeed.");
        var diagnostic = string.Join(Environment.NewLine, problems.Select(problem => problem.ShortErrorMessage))
            + Environment.NewLine
            + string.Join(Environment.NewLine, result.AnalysisErrors.Select(error => error.Message));
        Assert.AreEqual(7, problems.Count, diagnostic);
        Assert.AreEqual(
            1,
            problems.Count(problem => problem.ShortErrorMessage.Contains(UnprotectedSensitiveColumnsRule.Message, StringComparison.Ordinal)),
            "The exact exception must not suppress the existing classification-based finding.");
        Assert.AreEqual(
            6,
            problems.Count(problem => problem.ShortErrorMessage.Contains("potentially sensitive", StringComparison.Ordinal)),
            "The excluded keyword must not suppress another matching keyword, and an expired exception must not apply.");
    }

    [TestMethod]
    [DataRow("Custom", "None", true, true, 1)]
    [DataRow("Custom", "Medium", true, true, 1)]
    [DataRow("Custom", "None", false, true, 0)]
    [DataRow("Custom", "None", true, false, 0)]
    [DataRow("Public", "None", true, true, 0)]
    public void PolicyMatchingRespectsEnabledFlagsAndPublicLabel(
        string labelName, string rank, bool labelEnabled, bool typeEnabled, int expectedCount)
    {
        var path = Path.Combine(Path.GetTempPath(), "SRD0097-" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            File.WriteAllText(path, $$"""
                {
                  "Version": "2.0",
                  "Labels": {
                    "custom": { "DisplayName": "{{labelName}}", "Rank": "{{rank}}", "Enabled": {{labelEnabled.ToString().ToLowerInvariant()}} }
                  },
                  "InformationTypes": {
                    "custom": {
                      "DisplayName": "Custom type",
                      "Order": 1,
                      "RecommendedLabelId": "custom",
                      "Enabled": {{typeEnabled.ToString().ToLowerInvariant()}},
                      "Custom": true,
                      "Keywords": [{ "Pattern": "%secret%", "CanBeNumeric": false }]
                    }
                  }
                }
                """);

            using var model = new TSqlModel(SqlServerVersion.Sql150, new TSqlModelOptions());
            model.AddObjects("CREATE TABLE dbo.PolicyTest (Secret nvarchar(200) NULL);");
            var service = new CodeAnalysisServiceFactory().CreateAnalysisService(model.Version);
            var rule = service.GetRules()
                .Single(descriptor => descriptor.RuleId == UnprotectedSensitiveColumnsRule.RuleId)
                .Rule as UnprotectedSensitiveColumnsRule;
            Assert.IsNotNull(rule);
            rule.PolicyFile = path;

            var result = service.Analyze(model);
            Assert.IsTrue(result.AnalysisSucceeded, string.Join(Environment.NewLine, result.AnalysisErrors.Select(error => error.Message)));
            Assert.AreEqual(0, result.AnalysisErrors.Count, string.Join(Environment.NewLine, result.AnalysisErrors.Select(error => error.Message)));
            Assert.AreEqual(expectedCount, result.Problems.Count(problem => problem.RuleId == UnprotectedSensitiveColumnsRule.RuleId));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [TestMethod]
    public void ExceptionsFileCannotBeSelectedWithoutPolicy()
    {
        using var model = new TSqlModel(SqlServerVersion.Sql150, new TSqlModelOptions());
        model.AddObjects("CREATE TABLE dbo.PolicyTest (Email nvarchar(200) NULL);");

        var service = new CodeAnalysisServiceFactory().CreateAnalysisService(model.Version);
        var rule = service.GetRules()
            .Single(descriptor => descriptor.RuleId == UnprotectedSensitiveColumnsRule.RuleId)
            .Rule as UnprotectedSensitiveColumnsRule;
        Assert.IsNotNull(rule);
        rule.ExceptionsFile = Path.Combine(AppContext.BaseDirectory, "TestScripts", "SensitivityExceptions.json");

        var result = service.Analyze(model);

        Assert.IsTrue(
            result.AnalysisErrors.Any(error => error.Message.Contains("requires policy_file", StringComparison.Ordinal)),
            $"An exceptions-only configuration must surface an analysis error; found: {string.Join("; ", result.AnalysisErrors.Select(error => error.Message))}");
    }

    [TestMethod]
    public void PolicyCanBeSelectedByEditorConfigRelativeToItsOrigin()
    {
        var directory = Path.Combine(Path.GetTempPath(), "SRD0097-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);

        try
        {
            var sourcePath = Path.Combine(directory, "PolicyTest.sql");
            File.Copy(
                Path.Combine(AppContext.BaseDirectory, "TestScripts", "SensitivityPolicy.json"),
                Path.Combine(directory, "SensitivityPolicy.json"));
            File.WriteAllText(
                Path.Combine(directory, ".editorconfig"),
                """
                root = true

                [*.sql]
                sqlserver_rules.srd0097.policy_file = SensitivityPolicy.json
                """);

            using var model = new TSqlModel(SqlServerVersion.Sql150, new TSqlModelOptions());
            model.AddOrUpdateObjects(
                "CREATE TABLE dbo.PolicyTest (Email nvarchar(200) NULL);",
                sourcePath,
                new TSqlObjectOptions());

            var service = new CodeAnalysisServiceFactory().CreateAnalysisService(model.Version);
            var result = service.Analyze(model);
            var findings = result.Problems
                .Where(problem => problem.RuleId == UnprotectedSensitiveColumnsRule.RuleId)
                .ToList();

            Assert.IsTrue(result.AnalysisSucceeded);
            Assert.AreEqual(
                1,
                findings.Count,
                string.Join(Environment.NewLine, result.AnalysisErrors.Select(error => error.Message)));
            Assert.IsTrue(findings[0].ShortErrorMessage.Contains("potentially sensitive", StringComparison.Ordinal));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [TestMethod]
    public void UnsupportedPolicyVersionProducesAnalysisError()
    {
        using var model = new TSqlModel(SqlServerVersion.Sql150, new TSqlModelOptions());
        model.AddObjects("CREATE TABLE dbo.PolicyTest (Email nvarchar(200) NULL);");

        var service = new CodeAnalysisServiceFactory().CreateAnalysisService(model.Version);
        var rule = service.GetRules()
            .Single(descriptor => descriptor.RuleId == UnprotectedSensitiveColumnsRule.RuleId)
            .Rule as UnprotectedSensitiveColumnsRule;
        Assert.IsNotNull(rule);
        rule.PolicyFile = Path.Combine(AppContext.BaseDirectory, "TestScripts", "UnsupportedSensitivityPolicy.json");

        var result = service.Analyze(model);

        Assert.IsTrue(
            result.AnalysisErrors.Any(error => error.Message.Contains("Unsupported SRD0097 policy version", StringComparison.Ordinal)),
            $"An unsupported policy must surface an analysis error; found: {string.Join("; ", result.AnalysisErrors.Select(error => error.Message))}");
    }

    [TestMethod]
    public void UnsupportedPolicyFieldProducesAnalysisError()
    {
        using var model = new TSqlModel(SqlServerVersion.Sql150, new TSqlModelOptions());
        model.AddObjects("CREATE TABLE dbo.PolicyTest (Email nvarchar(200) NULL);");

        var service = new CodeAnalysisServiceFactory().CreateAnalysisService(model.Version);
        var rule = service.GetRules()
            .Single(descriptor => descriptor.RuleId == UnprotectedSensitiveColumnsRule.RuleId)
            .Rule as UnprotectedSensitiveColumnsRule;
        Assert.IsNotNull(rule);
        rule.PolicyFile = Path.Combine(AppContext.BaseDirectory, "TestScripts", "UnsupportedSensitivityFieldPolicy.json");

        var result = service.Analyze(model);

        Assert.IsTrue(
            result.AnalysisErrors.Any(error => error.Message.Contains("CaseSensitive", StringComparison.Ordinal)),
            $"An unsupported policy field must surface an analysis error; found: {string.Join(Environment.NewLine, result.AnalysisErrors.Select(error => error.Message))}");
    }

    [TestMethod]
    [DataRow("[")]
    [DataRow("email[")]
    [DataRow("[]")]
    [DataRow("[^]")]
    [DataRow("[z-a]")]
    public void InvalidLikePatternsProduceConfigurationErrors(string pattern)
    {
        var path = Path.Combine(Path.GetTempPath(), "SRD0097-" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            var policy = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "TestScripts", "SensitivityPolicy.json"));
            File.WriteAllText(path, policy.Replace("%email%", pattern, StringComparison.Ordinal));

            var exception = Assert.Throws<InvalidOperationException>(
                () => UnprotectedSensitiveColumnsRule.ValidatePolicyFiles(path, null));
            StringAssert.Contains(exception.Message, $"Invalid SQL LIKE pattern '{pattern}'", StringComparison.Ordinal);
            StringAssert.Contains(exception.Message, path, StringComparison.Ordinal);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
