using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using EditorConfig.Core;
using Microsoft.SqlServer.Dac.Model;

namespace SqlServer.Rules.Naming
{
    internal static class NamingRuleRegexConfiguration
    {
        private const string RulePrefix = "sqlserver_rules.srn0007.";
        private static readonly StringComparer SourcePathComparer = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;

        private static readonly ConcurrentDictionary<string, IReadOnlyDictionary<string, string>> SourcePropertiesCache = new(SourcePathComparer);
        private static readonly IReadOnlyDictionary<string, string> EmptyProperties = new Dictionary<string, string>();

        public static string GetConfiguredRegex(TSqlObject sqlObject, string ruleKey, string defaultRegex, RuleEditorConfig? editorConfig)
        {
            var properties = editorConfig == null ? GetEditorConfigProperties(sqlObject) : null;
            string? configuredRegex;
            var found = editorConfig != null
                ? editorConfig.TryGetValue(RulePrefix + ruleKey, out configuredRegex)
                : properties!.TryGetValue(RulePrefix + ruleKey, out configuredRegex);
            return found && !string.IsNullOrWhiteSpace(configuredRegex) ? configuredRegex! : defaultRegex;
        }

        private static IReadOnlyDictionary<string, string> GetEditorConfigProperties(TSqlObject sqlObject)
        {
            var sourcePath = sqlObject.GetSourceInformation()?.SourceName;
            if (string.IsNullOrWhiteSpace(sourcePath))
            {
                return EmptyProperties;
            }

            try
            {
                var fullPath = Path.GetFullPath(sourcePath);
                return SourcePropertiesCache.GetOrAdd(fullPath, static path => new EditorConfigParser().Parse(path).Properties);
            }
            catch (ArgumentException)
            {
                return EmptyProperties;
            }
            catch (PathTooLongException)
            {
                return EmptyProperties;
            }
            catch (IOException)
            {
                return EmptyProperties;
            }
            catch (NotSupportedException)
            {
                return EmptyProperties;
            }
            catch (System.Security.SecurityException)
            {
                return EmptyProperties;
            }
            catch (UnauthorizedAccessException)
            {
                return EmptyProperties;
            }
        }
    }
}
