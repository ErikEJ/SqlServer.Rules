using System;
using System.IO;
using System.Linq;
using System.Text;
using ErikEJ.DacFX.TSQLAnalyzer.Protocol;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlAnalyzerCli.Services;

namespace SqlServer.Rules.Tests.Docs;

[TestClass]
[TestCategory("Analyzer")]
public class ServerModeTests
{
    private const string SelectStarRuleId = "SqlServer.Rules.SRD0006";

    [TestMethod]
    public void AnalyzesInMemoryContent()
    {
        var request = new ServerRequest
        {
            Id = "1",
            Command = "analyze",
            Content = "CREATE PROCEDURE dbo.TestProc AS SELECT * FROM sys.objects;",
            SqlVersion = "Sql160",
        };

        var response = ServerMode.BuildAnalyzeResponse(request);

        Assert.AreEqual("1", response.Id);
        Assert.AreEqual("success", response.Status);
        Assert.IsNotNull(response.Problems);
        Assert.IsTrue(
            response.Problems.Any(p => p.Rule == SelectStarRuleId),
            "Expected in-memory content to be analyzed and produce the SELECT * problem.");
    }

    [TestMethod]
    public void PopulatesSeverityOnProblems()
    {
        var request = new ServerRequest
        {
            Id = "2",
            Command = "analyze",
            Content = "CREATE PROCEDURE dbo.TestProc AS SELECT * FROM sys.objects;",
            SqlVersion = "Sql160",
        };

        var response = ServerMode.BuildAnalyzeResponse(request);

        Assert.IsNotNull(response.Problems);
        Assert.IsNotEmpty(response.Problems);
        Assert.IsTrue(
            response.Problems.All(p => !string.IsNullOrEmpty(p.Severity)),
            "Every problem should carry a severity value.");
    }

    [TestMethod]
    public void ContentTakesPrecedenceOverPath()
    {
        var request = new ServerRequest
        {
            Id = "3",
            Command = "analyze",
            Path = "this-path-does-not-exist.sql",
            Content = "CREATE PROCEDURE dbo.TestProc AS SELECT * FROM sys.objects;",
            SqlVersion = "Sql160",
        };

        var response = ServerMode.BuildAnalyzeResponse(request);

        Assert.AreEqual("success", response.Status, "Content should be analyzed even when Path is invalid.");
    }

    [TestMethod]
    public void StillAnalyzesFileByPath()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".sql");

        try
        {
            File.WriteAllText(
                path,
                "CREATE PROCEDURE dbo.TestProc\r\nAS\r\nSELECT * FROM sys.objects;\r\n",
                new UTF8Encoding(true));

            var request = new ServerRequest
            {
                Id = "4",
                Command = "analyze",
                Path = path,
                SqlVersion = "Sql160",
            };

            var response = ServerMode.BuildAnalyzeResponse(request);

            Assert.AreEqual("success", response.Status);
            Assert.IsNotNull(response.Problems);
            Assert.IsTrue(
                response.Problems.Any(p => p.Rule == SelectStarRuleId),
                "Expected file-based analysis to keep working.");
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [TestMethod]
    public void EmptyContentTakesPrecedenceOverPath()
    {
        var request = new ServerRequest
        {
            Id = "5",
            Command = "analyze",
            Path = "this-path-does-not-exist.sql",
            Content = string.Empty,
            SqlVersion = "Sql160",
        };

        var response = ServerMode.BuildAnalyzeResponse(request);

        Assert.AreEqual("success", response.Status, "An empty content buffer should be analyzed in-memory and not fall back to the (invalid) Path.");
        Assert.IsNotNull(response.Problems);
        Assert.AreEqual(0, response.Problems.Count, "An empty buffer should produce no problems.");
    }

    [TestMethod]
    public void ReturnsErrorWhenNeitherPathNorContentProvided()
    {
        var request = new ServerRequest
        {
            Id = "6",
            Command = "analyze",
        };

        var response = ServerMode.BuildAnalyzeResponse(request);

        Assert.AreEqual("error", response.Status);
        Assert.IsNotNull(response.Error);
    }

    [TestMethod]
    public void ReturnsErrorWhenPathNotFound()
    {
        var request = new ServerRequest
        {
            Id = "7",
            Command = "analyze",
            Path = "this-path-does-not-exist.sql",
        };

        var response = ServerMode.BuildAnalyzeResponse(request);

        Assert.AreEqual("error", response.Status);
        Assert.IsNotNull(response.Error);
    }

    [TestMethod]
    public void UnprefixedRulesStringDisablesRule()
    {
        var request = new ServerRequest
        {
            Id = "8",
            Command = "analyze",
            Content = "CREATE PROCEDURE dbo.TestProc AS SELECT * FROM sys.objects;",
            SqlVersion = "Sql160",

            // Note: server-mode rules strings are NOT prefixed with "Rules:".
            Rules = "-" + SelectStarRuleId,
        };

        var response = ServerMode.BuildAnalyzeResponse(request);

        Assert.AreEqual("success", response.Status);
        Assert.IsNotNull(response.Problems);
        Assert.IsFalse(
            response.Problems.Any(p => p.Rule == SelectStarRuleId),
            "An unprefixed rules string should disable the requested rule.");
    }

    [TestMethod]
    public void NormalizeRulesAddsPrefixWhenMissing()
    {
        Assert.AreEqual(string.Empty, ServerMode.NormalizeRules(null));
        Assert.AreEqual(string.Empty, ServerMode.NormalizeRules("   "));
        Assert.AreEqual("Rules:-SqlServer.Rules.SRD0006", ServerMode.NormalizeRules("-SqlServer.Rules.SRD0006"));
        Assert.AreEqual("Rules:-SqlServer.Rules.SRD0006", ServerMode.NormalizeRules("Rules:-SqlServer.Rules.SRD0006"));
        Assert.AreEqual("Rules:-SqlServer.Rules.SRD0006", ServerMode.NormalizeRules("  Rules:-SqlServer.Rules.SRD0006  "));
    }
}
