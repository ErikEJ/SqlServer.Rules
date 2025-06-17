using System.ComponentModel;
using System.Text;
using ModelContextProtocol.Server;

namespace SqlAnalyzerCli.Services;

[McpServerToolType]
public sealed class AnalyzerTools
{
    [McpServerTool]
    [Description("Find design problems and bad practices in a SQL Server CREATE script")]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public static async Task<string> FindSqlScriptProblems(
        [Description("The SQL Server CREATE script to find design problems and bad practices in.")] string sqlScript)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
        if (string.IsNullOrWhiteSpace(sqlScript))
        {
            return "No script specified, make sure to select one.";
        }

        if (!sqlScript.Trim().StartsWith("CREATE ", StringComparison.OrdinalIgnoreCase))
        {
            return "The script must be an object creation script starting with 'CREATE'";
        }

        var factory = new ErikEJ.DacFX.TSQLAnalyzer.AnalyzerFactory(new() { Script = sqlScript });

        var result = factory.Analyze();

        var output = new StringBuilder();

        if (result.Result == null || result.Result.Problems.Count == 0)
        {
            return "No problems found.";
        }

        foreach (var problem in result.Result.Problems)
        {
            output.AppendLine(problem.Description);
        }

        return output.ToString();
    }
}
