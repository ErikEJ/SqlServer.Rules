using System.ComponentModel;
using System.Text;
using ModelContextProtocol.Server;

namespace SqlAnalyzerCli.Services;

[McpServerToolType]
public sealed class AnalyzerTools
{
    [McpServerTool]
    [Description("Analyzes a T-SQL (SQL Server) script for design, naming, performance problems and bad practices, using the SqlServer.Rules static code-analysis ruleset. Use this to review or lint SQL such as stored procedures, functions, views, CREATE/ALTER statements and any ad-hoc T-SQL before deploying them. Returns one line per problem found, each describing the rule namespace, rule id, the issue and an optional link to rule documentation.")]
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
            if (result.ModelErrors.Count > 0)
            {
                output.AppendLine("The script contains syntax errors and could not be analyzed.");
                return output.ToString();
            }

            output.AppendLine("No problems were found.");

            return output.ToString();
        }

        foreach (var problem in result.Result.Problems)
        {
            output.AppendLine(problem.ErrorMessageString);
        }

        return output.ToString();
    }
}
