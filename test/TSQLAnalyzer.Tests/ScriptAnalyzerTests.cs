using ErikEJ.DacFX.TSQLAnalyzer;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlServer.Rules.Tests.Docs;

[TestClass]
[TestCategory("Analyzer")]
public class ScriptAnalyzerTests
{
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
}
