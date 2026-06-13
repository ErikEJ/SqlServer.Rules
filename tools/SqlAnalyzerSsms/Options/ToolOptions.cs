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
        [DisplayName(@"Disable live code analysis")]
        [Description("Disable live static SQL code analysis")]
        [DefaultValue(false)]
        public bool DisableCodeAnalysis { get; set; }

        [Category("Code Analysis")]
        [DisplayName(@"Code Analysis rule settings")]
        [Description("Set the rules expression for live static SQL code analysis, for example: '-SqlServer.Rules.SRD0006;-SqlServer.Rules.SRN*'")]
        [DefaultValue(null)]
        public string? CodeAnalysisRuleSettings { get; set; }
    }
}