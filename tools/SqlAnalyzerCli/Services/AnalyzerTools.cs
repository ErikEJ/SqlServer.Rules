using System.ComponentModel;
using System.Text;
using ModelContextProtocol.Server;

namespace SqlAnalyzerCli.Services;

[McpServerToolType]
public sealed class AnalyzerTools
{
    [McpServerTool]
    [Description("Analyzes a T-SQL (SQL Server) script for design, naming, and performance problems and bad practices, using the SqlServer.Rules static code-analysis ruleset (the same rules used by DacFx/SSDT). Use this to review or lint SQL such as stored procedures, functions, views, and CREATE/ALTER statements before deploying them. Returns one line per problem found, each describing the rule, the location, and the issue.")]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public static async Task<string> FindSqlScriptProblems(
        [Description("The complete T-SQL script to analyze, for example one or more CREATE/ALTER statements for stored procedures, functions, views, or tables. Provide the full script text, not a file path.")] string sqlScript)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
        if (string.IsNullOrWhiteSpace(sqlScript))
        {
            return "No script specified, make sure to select one.";
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
