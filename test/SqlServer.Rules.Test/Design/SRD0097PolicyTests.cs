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
        model.AddObjects("""
            CREATE TABLE dbo.PolicyTest
            (
                Email nvarchar(200) NULL,
                EmailAddress nvarchar(200) NULL,
                MaskedEmail nvarchar(200) MASKED WITH (FUNCTION = 'email()') NULL,
                PhoneNumber int NULL,
                PolicyExcludedEmail nvarchar(200) NULL,
                [Policy.Excluded] nvarchar(200) NULL,
                ClassifiedEmail nvarchar(200) NULL,
                DisabledColumn nvarchar(200) NULL,
                ExpiredEmail nvarchar(200) NULL
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
        Assert.AreEqual(4, problems.Count, string.Join(Environment.NewLine, problems.Select(problem => problem.ShortErrorMessage)));
        Assert.AreEqual(
            1,
            problems.Count(problem => problem.ShortErrorMessage.Contains(UnprotectedSensitiveColumnsRule.Message, StringComparison.Ordinal)),
            "The exact exception must not suppress the existing classification-based finding.");
        Assert.AreEqual(
            3,
            problems.Count(problem => problem.ShortErrorMessage.Contains("potentially sensitive", StringComparison.Ordinal)),
            "The excluded keyword must not suppress another matching keyword, and an expired exception must not apply.");
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
}
