using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModelContextProtocol.Client;
using ModelContextProtocol.Extensions.Skills;
using ModelContextProtocol.Protocol;
using SqlAnalyzerCli.Services;

namespace SqlServer.Rules.Tests.Docs;

[TestClass]
[TestCategory("Analyzer")]
public class McpSkillsTests
{
    [TestMethod]
    public async Task ServerDiscoversAndServesProjectSetupSkill()
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var transport = new StdioClientTransport(new StdioClientTransportOptions
        {
            Command = "dotnet",
            Arguments = [typeof(AnalyzerTools).Assembly.Location, "-mcp"],
        });

        await using var client = await McpClient.CreateAsync(transport, cancellationToken: timeout.Token);

#pragma warning disable MCPEXP005
        Assert.IsTrue(client.ServerCapabilities.SupportsSkills());
        var skills = await client.ListSkillsAsync(cancellationToken: timeout.Token);
        var listed = skills.Single(s => s.Name == "sql-project-rules-setup");
        Assert.AreEqual("skill://sql-project-rules-setup/SKILL.md", listed.Uri);
        var skill = await client.GetSkillAsync(listed.Uri, cancellationToken: timeout.Token);
        var resource = await client.ReadSkillResourceAsync(skill, skill.Uri, cancellationToken: timeout.Token);
        var text = ((TextResourceContents)resource.Contents.Single()).Text;
        StringAssert.Contains(text, "MSBuild.Sdk.SqlProj", StringComparison.Ordinal);
        StringAssert.Contains(text, "ErikEJ.DacFX.SqlServer.Rules", StringComparison.Ordinal);
        StringAssert.Contains(text, "<RunSqlCodeAnalysis>True</RunSqlCodeAnalysis>", StringComparison.Ordinal);
#pragma warning restore MCPEXP005

        var tools = await client.ListToolsAsync(cancellationToken: timeout.Token);
        Assert.IsTrue(tools.Any(t => t.Name == "find_sql_script_problems"));
    }
}
