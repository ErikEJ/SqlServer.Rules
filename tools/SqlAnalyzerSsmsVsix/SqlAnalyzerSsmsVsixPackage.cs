global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using System;
global using Task = System.Threading.Tasks.Task;
using SqlAnalyzerSsms.Options;
using System.Runtime.InteropServices;
using System.Threading;

namespace SqlAnalyzerSsmsVsix
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [Guid(PackageGuids.SqlAnalyzerSsmsVsixString)]
    [ProvideOptionPage(typeof(OptionsProvider.GeneralOptions), "SQL Server Tools", "T-SQL Analyzer", 0, 0, true, SupportsProfiles = true)]
    [ProvideAutoLoad(AutoloadString, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideUIContextRule(
        AutoloadString,
        name: "auto load",
        expression: "sql1 | sql2",
        termNames: ["sql1", "sql2"],
        termValues: ["ActiveEditorContentType:SQL Server Tools", "ActiveEditorContentType:SQL"])]

    public sealed class SqlAnalyzerSsmsVsixPackage : ToolkitPackage
    {
        public const string AutoloadString = "2298A690-EE3E-42B0-BC1A-5A177B41CF0C";

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
        }
    }
}