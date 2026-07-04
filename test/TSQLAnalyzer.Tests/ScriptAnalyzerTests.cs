using System;
using System.IO;
using System.Linq;
using System.Text;
using ErikEJ.DacFX.TSQLAnalyzer;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlServer.Rules.Tests.Docs;

[TestClass]
[TestCategory("Analyzer")]
public class ScriptAnalyzerTests
{
    private const string SelectStarRuleId = "SqlServer.Rules.SRD0006";

    [TestMethod]
    public void CanCallApiWithScriptString()
    {
        // Arrange
        // Notice that script must be an object creation script.
        var script = @"CREATE PROCEDURE dbo.TestProc AS SELECT * FROM sys.objects";

        var options = new AnalyzerOptions
        {
            Script = script,
            SqlVersion = SqlServerVersion.Sql160,
        };
        var analyzer = new AnalyzerFactory(options);

        // Act
        var analysis = analyzer.Analyze();

        // Assert
        Assert.IsNotNull(analysis);
        Assert.IsNotNull(analysis.Result);
        Assert.IsNotEmpty(analysis.Result.Problems, "Expected problems but found none.");
    }

    [TestMethod]
    public void CanCallApiWithAlterProcedureScriptString()
    {
        var script = "ALTER PROCEDURE dbo.TestProc AS select * from sys.objects;";
        var analysis = Analyze(new AnalyzerOptions
        {
            Script = script,
            SqlVersion = SqlServerVersion.Sql160,
        });

        var selectStar = analysis.Result!.Problems.SingleOrDefault(p => p.RuleId == SelectStarRuleId);

        Assert.IsNotNull(selectStar, "Expected ALTER PROCEDURE input to be analyzed like CREATE PROCEDURE.");

        // The adjusted column must map back to the position of '*' in the original ALTER script.
        var expectedColumn = script.IndexOf('*', StringComparison.Ordinal) + 1;
        Assert.AreEqual(
            expectedColumn,
            analysis.GetAdjustedColumn(selectStar.StartLine, selectStar.StartColumn, selectStar.SourceName),
            "ALTER normalization should preserve the original source column.");
    }

    [TestMethod]
    public void CanCallApiWithCreateOrAlterProcedureScriptString()
    {
        var analysis = Analyze(new AnalyzerOptions
        {
            Script = "CREATE OR ALTER PROCEDURE dbo.TestProc AS select * from sys.objects;",
            SqlVersion = SqlServerVersion.Sql160,
        });

        Assert.IsTrue(
            analysis.Result!.Problems.Any(p => p.RuleId == SelectStarRuleId),
            "Expected CREATE OR ALTER PROCEDURE input to be analyzed.");
    }

    [TestMethod]
    public void CanCallApiWithAlterProcedureFile()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".sql");

        try
        {
            File.WriteAllText(
                path,
                "ALTER PROCEDURE dbo.TestProc\r\nAS\r\nSELECT * FROM sys.objects;\r\n",
                new UTF8Encoding(true));

            var analysis = Analyze(new AnalyzerOptions
            {
                Scripts = [path],
                SqlVersion = SqlServerVersion.Sql160,
            });

            Assert.IsTrue(
                analysis.Result!.Problems.Any(p => p.RuleId == SelectStarRuleId),
                "Expected ALTER PROCEDURE file input to be analyzed.");
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    private static AnalyzerResult Analyze(AnalyzerOptions options)
    {
        var analysis = new AnalyzerFactory(options).Analyze();

        Assert.IsNotNull(analysis);
        Assert.IsNotNull(analysis.Result);

        return analysis;
    }
}
