using System.Resources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Extensibility;

namespace SqlAnalyzer;

[VisualStudioContribution]
public class SqlAnalyzerExtension : Extension
{
    public const string SqlProjCapability = "MSBuild.Sdk.SqlProj.BuildTSqlScript";
    public const string MicrosoftBuildSqlCapability = "SQLProject";

    /// <inheritdoc/>
    public override ExtensionConfiguration ExtensionConfiguration => new()
    {
        Metadata = new(
                id: "SqlAnalyzer.abc6ba2-edd5-4419-8646-a55d0a83f7ff",
                version: this.ExtensionAssemblyVersion,
                publisherName: "ErikEJ",
                displayName: "T-SQL Analyzer",
                description: "T-SQL Analyzer, that analyzes your T-SQL object creation code for design, naming and performance issues using more than 140 rules.")
        {
            MoreInfo = "https://github.com/ErikEJ/SqlServer.Rules",
            Tags = ["SQL", "T-SQL", "Analyzer", "SQL Server"],
            Icon = "Images/sql-analysis.png",
            License = "license.txt",
            PreviewImage = "Images/sql-analysis.png",
            Preview = false,
            InstallationTargetVersion = "[17.14,19.0)",
        },
        LoadedWhen = ActivationConstraint.Or(
            ActivationConstraint.ActiveProjectCapability(ProjectCapability.Custom(SqlProjCapability)), // MSBuild.Sdk.SqlProj
            ActivationConstraint.ActiveProjectCapability(ProjectCapability.Custom(MicrosoftBuildSqlCapability)), // Microsoft.Build.Sql
            ActivationConstraint.ActiveProjectBuildProperty("DSP", ".*")), // Classic ".sqlproj"
    };

    /// <inheritdoc />
    protected override ResourceManager? ResourceManager => Strings.ResourceManager;

    /// <summary>
    /// Initialize local services owned by the extension. These services can be shared
    /// by various parts such as commands, editor listeners using dependency injection.
    /// </summary>
    /// <param name="serviceCollection">Service collection to add services to.</param>
    protected override void InitializeServices(IServiceCollection serviceCollection)
    {
        base.InitializeServices(serviceCollection);

        serviceCollection.AddSingleton<AnalyzerUtilities>();

        // As of now, any instance that ingests VisualStudioExtensibility is required to be added as a scoped
        // service.
        serviceCollection.AddScoped<SqlAnalyzerDiagnosticsService>();
    }
}
