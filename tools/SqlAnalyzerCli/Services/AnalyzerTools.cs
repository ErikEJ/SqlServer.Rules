using System.ComponentModel;
using ErikEJ.DacFX.TSQLAnalyzer.Protocol;
using ModelContextProtocol.Server;

namespace SqlAnalyzerCli.Services;

[McpServerToolType]
public sealed class AnalyzerTools
{
    [McpServerTool(UseStructuredContent = true)]
    [Description("Analyzes a T-SQL (SQL Server) script for design, naming, performance problems and bad practices, using the SqlServer.Rules static code-analysis ruleset. Use this to review or lint SQL such as stored procedures, functions, views, CREATE/ALTER statements and any ad-hoc T-SQL before deploying them. Returns a structured result with status, problems (rule, severity, location, message and documentation link), and nullable errors (kind, code and message). A success response with errors means analysis is incomplete; available problems are still returned. An error response contains an error message and must not be treated as a clean script.")]
    public static Task<ServerResponse> FindSqlScriptProblems(
        [Description("The complete T-SQL script to analyze, for example one or more CREATE/ALTER statements for stored procedures, functions, views, or tables. Provide the full script text, not a file path.")] string sqlScript)
    {
        if (string.IsNullOrWhiteSpace(sqlScript))
        {
            return Task.FromResult(new ServerResponse
            {
                Status = "error",
                Error = "No script specified, make sure to select one.",
            });
        }

        return Task.FromResult(ServerMode.BuildAnalyzeResponse(new ServerRequest
        {
            Command = "analyze",
            Content = sqlScript,
        }));
    }
}
