using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Settings;

namespace SqlAnalyzerVsix;

#pragma warning disable VSEXTPREVIEW_SETTINGS // The settings API is currently in preview and marked as experimental

public static class SettingDefinitions
{
#pragma warning disable CEE0027 // String not localized
    [VisualStudioContribution]
    private static SettingCategory AnalyzerCategory { get; } = new("sqlAnalyzerVsix", "T-SQL Analyzer");

    [VisualStudioContribution]
    public static Setting.Boolean RunAnalysis { get; } = new("runAnalysis", "Run static T-SQL analysis", AnalyzerCategory, defaultValue: true)
    {
        Description = "Enable or disable static T-SQL analysis. When enabled, the extension will analyze your T-SQL code for design, naming and performance issues using more than 140 rules",
    };

    [VisualStudioContribution]
    public static Setting.String Rules { get; } = new(
        "rules",
        "Rule exceptions",
        AnalyzerCategory,
        defaultValue: string.Empty)
    {
        Description = "Set the rules expression for live static SQL code analysis when no SQL project rule configuration is available (for example: '+!SqlServer.Rules.SRD0006;-SqlServer.Rules.SRN*')",
    };

    [VisualStudioContribution]
    public static Setting.Enum SqlEngineVersion { get; } = new(
        "sqlVersion",
        "SQL engine version",
        AnalyzerCategory,
        [
            new("Sql170", "SQL Server 2025 and Managed Instance"),
            new("Sql160", "SQL Server 2022"),
            new("SqlAzure", "Azure SQL Database"),
            new("SqlDw", "Microsoft Azure SQL Data Warehouse"),
            new("SqlServerless", "Azure Synapse Analytics Serverless SQL Pool"),
            new("SqlDwUnified", "Fabric Data Warehouse"),
            new("Sql150", "SQL Server 2019"),
            new("Sql140", "SQL Server 2017"),
            new("Sql130", "SQL Server 2016"),
            new("Sql120", "SQL Server 2014"),
            new("Sql110", "SQL Server 2012"),
            new("Sql100", "SQL Server 2008"),
            new("Sql90", "SQL Server 2005"),
        ],
        defaultValue: "Sql170")
    {
        Description = "Set the SQL Server dialect used in analysis",
    };

#pragma warning restore CEE0027 // String not localized
}
