using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;
using SqlAnalyzerSsms.Options;

namespace SqlAnalyzerSsms.Helpers
{
    internal static class UpdateChecker
    {
        private static readonly XNamespace AtomNamespace = "http://www.w3.org/2005/Atom";
        private static readonly XNamespace VsixNamespace = "http://schemas.microsoft.com/developer/vsx-syndication-schema/2010";

        private static string GetLastCheckFilePath(string extensionId)
        {
            var folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "SqlAnalyzerSsms");
            Directory.CreateDirectory(folder);
            return Path.Combine(folder, $"{extensionId}-lastcheck.txt");
        }

        private static bool HasCheckedToday(string extensionId)
        {
            try
            {
                var filePath = GetLastCheckFilePath(extensionId);
                if (!File.Exists(filePath))
                {
                    return false;
                }

                var content = File.ReadAllText(filePath).Trim();
                return DateTime.TryParseExact(content, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var lastCheck) &&
                       lastCheck.Date == DateTime.UtcNow.Date;
            }
#pragma warning disable CA1031 // Do not catch general exception types - we don't want to crash over update check file issues
            catch (Exception ex)
            {
                ex.Log();
                return false;
            }
#pragma warning restore CA1031 // Do not catch general exception types
        }

        private static void SaveLastCheckDate(string extensionId)
        {
            try
            {
                File.WriteAllText(GetLastCheckFilePath(extensionId), DateTime.UtcNow.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            }
#pragma warning disable CA1031 // Do not catch general exception types - we don't want to crash over update check file issues
            catch (Exception ex)
            {
                ex.Log();
            }
#pragma warning restore CA1031 // Do not catch general exception types
        }

        public static async Task CheckForUpdatesAsync(string extensionId, string currentVersion, string extensionName)
        {
            try
            {
                var options = await ToolOptions.GetLiveInstanceAsync();
                if (!options.CheckForUpdates)
                {
                    return;
                }

                if (HasCheckedToday(extensionId))
                {
                    return;
                }

                SaveLastCheckDate(extensionId);

                var feedUrl = $"https://www.vsixgallery.com/feed/extension/{extensionId}";
                string feedContent;

                using (var httpClient = new System.Net.Http.HttpClient { Timeout = TimeSpan.FromSeconds(10) })
                {
                    feedContent = await httpClient.GetStringAsync(new Uri(feedUrl)).ConfigureAwait(false);
                }

                var latestVersion = ParseVersionFromFeed(feedContent);
                if (latestVersion != null && IsNewerVersion(latestVersion, currentVersion))
                {
                    await ShowUpdateNotificationAsync(extensionId, extensionName, latestVersion);
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types - we don't want to crash over update check issues
            catch (Exception ex)
            {
                await ex.LogAsync();
            }
#pragma warning restore CA1031 // Do not catch general exception types
        }

        private static string? ParseVersionFromFeed(string feedContent)
        {
            var doc = XDocument.Parse(feedContent);
            var firstEntry = doc.Root?.Element(AtomNamespace + "entry");

            if (firstEntry == null)
            {
                return null;
            }

            var vsixElement = firstEntry.Element(VsixNamespace + "Vsix");
            var versionElement = vsixElement?.Element(VsixNamespace + "Version");
            if (versionElement != null && Version.TryParse(versionElement.Value, out _))
            {
                return versionElement.Value;
            }

            return null;
        }

        private static bool IsNewerVersion(string latestVersion, string currentVersion)
        {
            if (Version.TryParse(latestVersion, out var latest) &&
                Version.TryParse(currentVersion, out var current))
            {
                return latest > current;
            }

            return false;
        }

        private static async Task ShowUpdateNotificationAsync(string extensionId, string extensionName, string newVersion)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var downloadUrl = $"https://www.vsixgallery.com/extension/{extensionId}";

            var model = new InfoBarModel(
                $"{extensionName} {newVersion} is available.",
                [new InfoBarHyperlink("Download")],
                KnownMonikers.StatusInformation);

            var infoBar = await VS.InfoBar.CreateAsync(model);
            if (infoBar != null)
            {
                infoBar.ActionItemClicked += (sender, e) =>
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo(downloadUrl) { UseShellExecute = true });
                    }
#pragma warning disable CA1031 // Do not catch general exception types - we don't want to crash over a notification click
                    catch (Exception ex)
                    {
                        ex.Log();
                    }
#pragma warning restore CA1031 // Do not catch general exception types
                    finally
                    {
                        if (sender is InfoBar bar)
                        {
                            bar.Close();
                        }
                    }
                };

                await infoBar.TryShowInfoBarUIAsync();
            }
        }
    }
}
