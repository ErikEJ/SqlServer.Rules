using System.Net;
using System.Threading.RateLimiting;
using ErikEJ.DacFX.TSQLAnalyzer;
using ErikEJ.DacFX.TSQLAnalyzer.Extensions;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.SqlServer.Dac.Model;

const int MaxScriptLength = 8 * 1024;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(o => o.Limits.MaxRequestBodySize = 64 * 1024);

builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.AddPolicy("analyze", ctx => RateLimitPartition.GetFixedWindowLimiter(
        ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1) }));
});

var app = builder.Build();

var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
};

foreach (var proxy in builder.Configuration.GetSection("ForwardedHeaders:KnownProxies").Get<string[]>() ?? [])
{
    forwardedHeadersOptions.KnownProxies.Add(IPAddress.Parse(proxy));
}

foreach (var network in builder.Configuration.GetSection("ForwardedHeaders:KnownNetworks").Get<string[]>() ?? [])
{
    forwardedHeadersOptions.KnownIPNetworks.Add(System.Net.IPNetwork.Parse(network));
}

app.UseForwardedHeaders(forwardedHeadersOptions);
app.UseRateLimiter();
app.UseDefaultFiles();
app.UseStaticFiles();

var gate = new SemaphoreSlim(2);

app.MapGet("/healthz", () => Results.Ok("ok"));

app.MapPost("/api/analyze", async (AnalyzeRequest request, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(request.Script))
    {
        return Results.BadRequest(new { error = "Script is required." });
    }

    if (request.Script.Length > MaxScriptLength)
    {
        return Results.BadRequest(new { error = $"Script is limited to {MaxScriptLength:N0} characters (8 KB)." });
    }

    if (!Enum.TryParse<SqlServerVersion>(request.SqlVersion ?? "Sql170", true, out var version))
    {
        return Results.BadRequest(new { error = "Unknown SQL Server version." });
    }

    if (!await gate.WaitAsync(TimeSpan.FromSeconds(10), ct))
    {
        return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
    }

    try
    {
        var options = new AnalyzerOptions
        {
            Script = request.Script,
            SqlVersion = version,
            Rules = string.IsNullOrWhiteSpace(request.Rules) ? null : request.Rules,
        };

        var analysisTask = Task.Run(() => new AnalyzerFactory(options).Analyze(), ct);
        _ = analysisTask.ContinueWith(
            _ => gate.Release(),
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);

        var analysis = await analysisTask.WaitAsync(TimeSpan.FromSeconds(30), ct);

        var modelErrors = analysis.ModelErrors.Values.Select(e => e.Message).ToList();

        if (analysis.Result is null)
        {
            return Results.Ok(new AnalyzeResponse([], modelErrors, []));
        }

        var problems = analysis.Result.Problems
            .Select(p =>
            {
                var (endLine, endColumn) = p.GetEndPosition(analysis);
                return new ProblemDto(
                    p.RuleId,
                    p.Description,
                    p.Severity.ToString(),
                    p.StartLine,
                    analysis.GetAdjustedColumn(p.StartLine, p.StartColumn, p.SourceName),
                    endLine,
                    endColumn,
                    p.GetMicrosoftRuleHelpLink());
            })
            .OrderBy(p => p.Line)
            .ThenBy(p => p.Column)
            .ToList();

        var errors = analysis.Result.GetAllErrors().Select(e => e.GetOutputMessage()).ToList();

        return Results.Ok(new AnalyzeResponse(problems, modelErrors, errors));
    }
    catch (TimeoutException)
    {
        return Results.Json(new { error = "Analysis timed out." }, statusCode: StatusCodes.Status408RequestTimeout);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
}).RequireRateLimiting("analyze");

app.Run();

internal sealed record AnalyzeRequest(string? Script, string? SqlVersion, string? Rules);

internal sealed record ProblemDto(
    string Rule, string Description, string Severity, int Line, int Column, int EndLine, int EndColumn, string? HelpLink);

internal sealed record AnalyzeResponse(
    IReadOnlyList<ProblemDto> Problems, IReadOnlyList<string> ModelErrors, IReadOnlyList<string> Errors);
