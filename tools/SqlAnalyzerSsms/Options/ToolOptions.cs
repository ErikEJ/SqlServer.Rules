using System.ComponentModel;
using Community.VisualStudio.Toolkit;

namespace SqlAnalyzerSsms.Options
{
    public class ToolOptions : BaseOptionModel<ToolOptions>
    {
        [Category("General")]
        [DisplayName(@"Check for updates")]
        [Description("Check for updates to the extension on startup")]
        [DefaultValue(true)]
        public bool CheckForUpdates { get; set; } = true;

        [Category("Code Analysis")]
        [DisplayName(@"Run static T-SQL analysis")]
        [Description("Enable or disable static T-SQL analysis. When enabled, the extension will analyze your T-SQL code for design, naming and performance issues using the built-in rule set.")]
        [DefaultValue(true)]
        public bool RunAnalysis { get; set; } = true;

        [Category("Code Analysis")]
        [DisplayName(@"Rule exceptions")]
        [Description("Set the rules expression for live static SQL code analysis when no SQL project rule configuration is available (for example: '-SqlServer.Rules.SRD0006;-SqlServer.Rules.SRN*').")]
        [DefaultValue(null)]
        public string? CodeAnalysisRuleSettings { get; set; }

        [Category("Code Analysis")]
        [DisplayName(@"SQL engine version")]
        [Description("Set the SQL Server dialect used in analysis, for example Sql170 for SQL Server 2025.")]
        [DefaultValue(null)]
        public string? SqlEngineVersion { get; set; }
    }
}