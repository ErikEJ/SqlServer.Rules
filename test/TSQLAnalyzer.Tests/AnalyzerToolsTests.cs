using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlAnalyzerCli.Services;

namespace SqlServer.Rules.Tests.Docs;

[TestClass]
[TestCategory("Analyzer")]
public class AnalyzerToolsTests
{
    [TestMethod]
    public async Task ReturnsTypedFindingsForValidScript()
    {
        var response = await AnalyzerTools.FindSqlScriptProblems("CREATE PROCEDURE dbo.TestProc AS SELECT * FROM sys.objects;");

        Assert.AreEqual("success", response.Status);
        Assert.IsNull(response.Error);
        Assert.IsNull(response.Errors);
        Assert.IsNotNull(response.Problems);
        var problem = response.Problems.First(p => p.Rule == "SqlServer.Rules.SRD0006");
        Assert.AreEqual(1, problem.Line);
        Assert.IsTrue(problem.Column > 0);
        Assert.IsFalse(string.IsNullOrWhiteSpace(problem.Message));
        Assert.IsFalse(string.IsNullOrWhiteSpace(problem.Severity));
        Assert.IsFalse(string.IsNullOrWhiteSpace(problem.HelpLink));
    }

    [TestMethod]
    public async Task ReturnsModelErrorsInsteadOfCleanResult()
    {
        var response = await AnalyzerTools.FindSqlScriptProblems("CREATE PROCEDURE dbo.TestProc AS RAISERROR 13001 'Removed syntax';");

        Assert.AreEqual("success", response.Status);
        Assert.IsNotNull(response.Errors);
        Assert.IsTrue(response.Errors.Any(e => e.Kind == "model" && e.Code == "SQL46010" && !string.IsNullOrWhiteSpace(e.Message)));
    }

    [TestMethod]
    public async Task PreservesFindingsAlongsideModelErrors()
    {
        var response = await AnalyzerTools.FindSqlScriptProblems("CREATE PROCEDURE dbo.TestProc AS SELECT * FROM sys.objects;\nGO\nCREATE PROCEDURE dbo.InvalidProc AS RAISERROR 13001 'Removed syntax';");

        Assert.AreEqual("success", response.Status);
        Assert.IsNotNull(response.Errors);
        Assert.IsTrue(response.Errors.Any(e => e.Kind == "model" && e.Code == "SQL46010"));
        Assert.IsNotNull(response.Problems);
        Assert.IsTrue(response.Problems.Any(p => p.Rule == "SqlServer.Rules.SRD0006"));
    }

    [TestMethod]
    public async Task ReturnsErrorForMissingScript()
    {
        var response = await AnalyzerTools.FindSqlScriptProblems(" ");

        Assert.AreEqual("error", response.Status);
        Assert.AreEqual("No script specified, make sure to select one.", response.Error);
        Assert.IsNull(response.Problems);
    }
}
