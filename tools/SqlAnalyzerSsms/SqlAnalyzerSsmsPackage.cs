using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using SqlAnalyzerSsms.Helpers;
using SqlAnalyzerSsms.Options;

namespace SqlAnalyzerSsms;

[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
[Guid(SqlAnalyzerSsmsPackage.PackageGuidString)]

[InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
[ProvideOptionPage(typeof(OptionsProvider.GeneralOptions), "T-SQL Analyzer", "General", 0, 0, true, SupportsProfiles = true)]
[ProvideAutoLoad(AutoloadString, PackageAutoLoadFlags.BackgroundLoad)]
[ProvideUIContextRule(
    AutoloadString,
    name: "auto load",
    expression: "sql1 | sql2",
    termNames: ["sql1", "sql2"],
    termValues: ["ActiveEditorContentType:SQL Server Tools", "ActiveEditorContentType:SQL"])]

public sealed class SqlAnalyzerSsmsPackage : AsyncPackage
{
    public const string PackageGuidString = "c6c41724-b12a-4f86-a53d-a6eb70dc6c81";

    public const string AutoloadString = "2298A690-EE3E-42B0-BC1A-5A177B41CF0B";

    protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
    {
        // When initialized asynchronously, the current thread may be a background thread at this point.
        // Do any initialization that requires the UI thread after switching to the UI thread.
        await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

        _ = UpdateChecker.CheckForUpdatesAsync(
            Vsix.Id,
            FileVersionInfo.GetVersionInfo(typeof(SqlAnalyzerSsmsPackage).Assembly.Location).FileVersion,
            Vsix.Name);
    }
}
