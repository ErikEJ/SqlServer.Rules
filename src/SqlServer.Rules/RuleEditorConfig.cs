using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using EditorConfig.Core;

namespace SqlServer.Rules
{
    /// <summary>
    /// An explicitly selected analysis-wide EditorConfig file.
    /// </summary>
    public sealed class RuleEditorConfig
    {
        private readonly IReadOnlyDictionary<string, string> properties;
        private readonly string directory;

        private RuleEditorConfig(string directory, IReadOnlyDictionary<string, string> properties)
        {
            this.directory = directory;
            this.properties = properties;
        }

        /// <summary>
        /// Loads settings from the explicitly selected file's [*.sql] sections.
        /// </summary>
        /// <param name="path">The EditorConfig file path.</param>
        /// <returns>The analysis-wide configuration.</returns>
        public static RuleEditorConfig Load(string path)
        {
            var fullPath = Path.GetFullPath(path);
            var text = File.ReadAllText(fullPath);
            using (var validationReader = new StringReader(text))
            {
                var lineNumber = 0;
                string? line;
                while ((line = validationReader.ReadLine()) != null)
                {
                    lineNumber++;
                    var trimmed = line.Trim();
                    if (trimmed.Length == 0 || trimmed.StartsWith("#", StringComparison.Ordinal) || trimmed.StartsWith(";", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    if (!Regex.IsMatch(line, @"^\s*\[(([^#;]|\\#|\\;)+)\]\s*([#;].*)?$")
                        && !Regex.IsMatch(line, @"^\s*[\w\.\-_]+\s*[=:]\s*.*$"))
                    {
                        throw new FormatException($"Invalid EditorConfig syntax in '{fullPath}' at line {lineNumber}.");
                    }
                }
            }

            using var reader = new StringReader(text);
            var file = EditorConfigFile.Parse(reader);
            var properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var section in file.Sections)
            {
                if (!string.Equals(section.Glob, "*.sql", StringComparison.Ordinal))
                {
                    continue;
                }

                foreach (var property in section)
                {
                    if (string.Equals(property.Value, "unset", StringComparison.OrdinalIgnoreCase))
                    {
                        properties.Remove(property.Key);
                    }
                    else
                    {
                        properties[property.Key] = property.Value;
                    }
                }
            }

            return new RuleEditorConfig(Path.GetDirectoryName(fullPath)!, properties);
        }

        internal bool TryGetValue(string key, out string value) => properties.TryGetValue(key, out value!);

        internal string? GetFilePath(string key)
        {
            if (!TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return Path.GetFullPath(Path.IsPathRooted(value) ? value : Path.Combine(directory, value));
        }
    }
}
