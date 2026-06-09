using System;
using System.Threading.Tasks;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;

namespace SqlAnalyzerSsms.Extensions
{
    internal static class ProjectExtension
    {
        public static async Task<(bool Run, string Rules, string SqlVersion)> IsInSqlProjAsync(this Project project)
        {
            var rulesExpression = await project.GetAttributeAsync("SqlCodeAnalysisRules")
                ?? await project.GetAttributeAsync("CodeAnalysisRules")
                ?? string.Empty;
            var runCodeAnalysisValue = await project.GetAttributeAsync("RunSqlCodeAnalysis") ?? string.Empty;
            var runCodeAnalysis = string.Equals(runCodeAnalysisValue, "True", StringComparison.OrdinalIgnoreCase);

            var serverVersion = await project.GetSqlServerVersionAsync();

            return (runCodeAnalysis, rulesExpression, serverVersion);
        }

        public static async Task<string> GetSqlServerVersionAsync(this Project project)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            // MsBuild.Sdk.SqlProj uses SqlServerVersion directly (e.g. "Sql160")
            var sqlServerVersion = await project.GetAttributeAsync("SqlServerVersion") ?? string.Empty;
            if (!string.IsNullOrEmpty(sqlServerVersion))
            {
                return sqlServerVersion;
            }

            // Classic .sqlproj / Microsoft.Build.Sql uses DSP property
            var dsp = await project.GetAttributeAsync("DSP") ?? string.Empty;
            var version = ParseVersionFromDsp(dsp);
            return version ?? "Sql160";
        }

        private static string? ParseVersionFromDsp(string dsp)
        {
            if (string.IsNullOrEmpty(dsp))
            {
                return null;
            }

            var trimmedDsp = dsp.Replace("V12", string.Empty);

            // Extract version from e.g. "Microsoft.Data.Tools.Schema.Sql.Sql160DatabaseSchemaProvider"
            // Microsoft.Data.Tools.Schema.Sql.SqlAzureV12DatabaseSchemaProvider (Azure SQL Database)
            var marker = "DatabaseSchemaProvider";
            var markerIndex = trimmedDsp.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (markerIndex <= 0)
            {
                return null;
            }

            var beforeMarker = trimmedDsp.Substring(0, markerIndex);
            var lastDot = beforeMarker.LastIndexOf('.');
            if (lastDot < 0 || lastDot >= beforeMarker.Length - 1)
            {
                return null;
            }

            return beforeMarker.Substring(lastDot + 1);
        }
    }
}
