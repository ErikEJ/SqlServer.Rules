using System.Reflection;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using Spectre.Console;

namespace SqlAnalyzerCli.Services;

internal static class PackageService
{
    public static NuGetVersion CurrentPackageVersion()
    {
        return new NuGetVersion(typeof(NuGetVersion).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion);
    }

    public static async Task CheckForPackageUpdateAsync()
    {
        try
        {
            var logger = new NullLogger();
            var cancellationToken = CancellationToken.None;

            using var cache = new SourceCacheContext();
            var repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
            var resource = await repository.GetResourceAsync<PackageMetadataResource>().ConfigureAwait(false);

            var packages = await resource.GetMetadataAsync(
                "ErikEJ.DacFX.TSQLAnalyzer.Cli",
                includePrerelease: false,
                includeUnlisted: false,
                cache,
                logger,
                cancellationToken).ConfigureAwait(false);

            var latestVersion = packages.Select(v => v.Identity.Version).MaxBy(v => v);
            if (latestVersion != null && latestVersion > CurrentPackageVersion())
            {
                DisplayService.MarkupLine();
                DisplayService.MarkupLine("Your are not using the latest version of the tool, please update to get the latest bug fixes, features and support.", Color.Yellow);
                DisplayService.MarkupLine($"Latest version is '{latestVersion}'", Color.Yellow);
                DisplayService.MarkupLine($"Run 'dotnet tool install --global ErikEJ.DacFX.TSQLAnalyzer.Cli' to get the latest version.", Color.Yellow);
            }
        }
#pragma warning disable CA1031
        catch (Exception)
        {
            // Ignore
        }
#pragma warning restore CA1031
    }
}