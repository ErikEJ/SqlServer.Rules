// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace MarkdownLinter;

using System.Resources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Editor;

/// <summary>
/// Extension entry point for Markdown Linter sample extension showcasing new
/// out of proc Visual Studio Extensibilty APIs.
/// </summary>
[VisualStudioContribution]
public class SqlAnalyzerExtension : Extension
{
    public const string SqlProjCapability = "MSBuild.Sdk.SqlProj.BuildTSqlScript";

    /// <inheritdoc/>
    public override ExtensionConfiguration ExtensionConfiguration => new()
    {
        Metadata = new(
                id: "SqlAnalyzer.abc6ba2-edd5-4419-8646-a55d0a83f799",
                version: this.ExtensionAssemblyVersion,
                publisherName: "Microsoft",
                displayName: "SQL Analyzer Sample Extension",
                description: "Sample SQL analyzer extension"),
        LoadedWhen = ActivationConstraint.ActiveProjectCapability(ProjectCapability.Custom(SqlProjCapability)),
        //{
        //    MoreInfo = "https://github.com/ErikEJ/SqlServer.Rules",
        //    Tags = ["SQL", "T-SQL", "Analyzer", "SQL Server"],
        //    //Icon = "Resources/sql-analysis.png",
        //    //PreviewImage = "Resources/sql-analysis.png",
        //},
        //LoadedWhen = ActivationConstraint.Or(
        //    ActivationConstraint.ActiveProjectCapability(ProjectCapability.Custom(SqlProjCapability)),
        //    ActivationConstraint.ActiveProjectFlavor(new System.Guid("A9ACE9BB-CECE-4E62-9AA4-C7E7C5BD2124"))),
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

        // Add linter utilities as a singleton, it depends on settings observer.
        serviceCollection.AddSingleton<AnalyzerUtilities>();

        // As of now, any instance that ingests VisualStudioExtensibility is required to be added as a scoped
        // service.
        serviceCollection.AddScoped<SqlAnalyzerDiagnosticsService>();
    }
}
