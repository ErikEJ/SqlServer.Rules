using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using EditorConfig.Core;
using Microsoft.SqlServer.Dac.Model;

namespace SqlServer.Rules.Design
{
    internal sealed class SensitivityPolicy
    {
        private static readonly StringComparer IdentifierComparer = StringComparer.InvariantCultureIgnoreCase;
        private readonly List<PolicyMatch> matches;
        private readonly Dictionary<string, PolicyException> exceptions;

        private SensitivityPolicy(List<PolicyMatch> matches, Dictionary<string, PolicyException> exceptions)
        {
            this.matches = matches;
            this.exceptions = exceptions;
        }

        public static (string? PolicyPath, string? ExceptionsPath) GetConfiguredPaths(TSqlObject sourceObject)
        {
            var sourcePath = sourceObject.GetSourceInformation()?.SourceName;
            if (string.IsNullOrWhiteSpace(sourcePath))
            {
                return (null, null);
            }

            var fullSourcePath = Path.GetFullPath(sourcePath);
            var parser = new EditorConfigParser();
            var configuration = parser.Parse(fullSourcePath);
            var policyKey = "sqlserver_rules.srd0097.policy_file";
            var exceptionsKey = "sqlserver_rules.srd0097.exceptions_file";
            var policyPath = ResolveSetting(parser, fullSourcePath, configuration.Properties, policyKey);
            var exceptionsPath = ResolveSetting(parser, fullSourcePath, configuration.Properties, exceptionsKey);

            if (exceptionsPath != null && policyPath == null)
            {
                throw new InvalidOperationException("SRD0097 exceptions_file requires policy_file.");
            }

            return (policyPath, exceptionsPath);
        }

        public static SensitivityPolicy Load(string policyPath, string? exceptionsPath, DateTime utcNow)
        {
            var document = ReadJson<PolicyDocument>(policyPath);
            if (!string.Equals(document.Version, "2.0", StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Unsupported SRD0097 policy version '{document.Version}' in '{policyPath}'. Only version 2.0 is supported.");
            }

            if (document.Labels == null || document.InformationTypes == null)
            {
                throw new InvalidOperationException($"SRD0097 policy '{policyPath}' must contain Labels and InformationTypes.");
            }

            var matches = new List<PolicyMatch>();
            foreach (var informationType in document.InformationTypes.Values.OrderBy(x => x.Order))
            {
                if (!informationType.Enabled)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(informationType.RecommendedLabelId)
                    || !document.Labels.TryGetValue(informationType.RecommendedLabelId, out var label))
                {
                    throw new InvalidOperationException($"Information type '{informationType.DisplayName}' in '{policyPath}' references an unknown RecommendedLabelId.");
                }

                if (!label.Enabled
                    || string.Equals(label.DisplayName, "Public", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(label.Rank, "None", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                foreach (var keyword in informationType.Keywords ?? new List<PolicyKeyword>())
                {
                    if (keyword.Pattern == null)
                    {
                        throw new InvalidOperationException($"Information type '{informationType.DisplayName}' in '{policyPath}' contains a keyword without a Pattern.");
                    }

                    if (keyword.Excluded == true)
                    {
                        continue;
                    }

                    matches.Add(new PolicyMatch(
                        informationType.DisplayName ?? informationType.RecommendedLabelId,
                        keyword.Pattern,
                        keyword.CanBeNumeric,
                        CreateLikeRegex(keyword.Pattern, policyPath)));
                }
            }

            var exceptions = exceptionsPath == null
                ? new Dictionary<string, PolicyException>(IdentifierComparer)
                : LoadExceptions(exceptionsPath, utcNow);

            return new SensitivityPolicy(matches, exceptions);
        }

        public PolicyMatch? FindMatch(TSqlObject column)
        {
            var columnName = column.Name.Parts.Last();
            var numeric = IsNumeric(column);
            return matches.FirstOrDefault(match =>
                (match.CanBeNumeric || !numeric)
                && match.Regex.IsMatch(columnName));
        }

        public bool IsExcluded(TSqlObject column)
            => exceptions.ContainsKey(GetObjectName(column));

        private static string? ResolveSetting(
            EditorConfigParser parser,
            string sourcePath,
            IReadOnlyDictionary<string, string> properties,
            string key)
        {
            if (!properties.TryGetValue(key, out var configuredPath) || string.IsNullOrWhiteSpace(configuredPath))
            {
                return null;
            }

            var origin = parser.GetConfigurationFilesTillRoot(sourcePath)
                .OfType<EditorConfigFile>()
                .SelectMany(file => file.Sections.Select(section => (file, section)))
                .LastOrDefault(item =>
                    item.section.TryGetValue(key, out var value)
                    && string.Equals(value, configuredPath, StringComparison.Ordinal)
                    && GlobMatcher.Create(item.section.Glob, new GlobMatcherOptions
                    {
                        MatchBase = true,
                        Dot = true,
                        AllowWindowsPaths = true,
                    }).IsMatch(sourcePath))
                .file;

            var path = Path.IsPathRooted(configuredPath)
                ? configuredPath
                : Path.Combine(origin?.Directory ?? Path.GetDirectoryName(sourcePath)!, configuredPath);

            return Path.GetFullPath(path);
        }

        private static Dictionary<string, PolicyException> LoadExceptions(string path, DateTime utcNow)
        {
            var document = ReadJson<ExceptionDocument>(path);
            var result = new Dictionary<string, PolicyException>(IdentifierComparer);

            foreach (var exception in document.Exceptions ?? new List<PolicyExceptionEntry>())
            {
                if (string.IsNullOrWhiteSpace(exception.Column) || string.IsNullOrWhiteSpace(exception.Reason))
                {
                    throw new InvalidOperationException($"Each SRD0097 exception in '{path}' must specify a column and a non-empty reason.");
                }

                var name = ParseObjectName(exception.Column, path);
                DateTime? expires = null;
                if (!string.IsNullOrWhiteSpace(exception.Expires))
                {
                    if (!DateTime.TryParseExact(
                        exception.Expires,
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var expiryDate))
                    {
                        throw new InvalidOperationException($"Invalid expires date '{exception.Expires}' in SRD0097 exceptions file '{path}'. Use yyyy-MM-dd.");
                    }

                    expires = expiryDate.Date;
                }

                var normalizedName = string.Join(".", name);
                var entry = new PolicyException(exception.Reason, exception.ApprovedBy, expires);

                if (result.TryGetValue(normalizedName, out var existing)
                    && (existing.Reason != entry.Reason
                        || existing.ApprovedBy != entry.ApprovedBy
                        || existing.Expires != entry.Expires))
                {
                    throw new InvalidOperationException($"Conflicting SRD0097 exceptions for '{exception.Column}' in '{path}'.");
                }

                if (expires == null || utcNow.Date <= expires.Value)
                {
                    result[normalizedName] = entry;
                }
            }

            return result;
        }

        private static string[] ParseObjectName(string value, string path)
        {
            var parts = new List<string>();
            var index = 0;

            while (index < value.Length)
            {
                while (index < value.Length && char.IsWhiteSpace(value[index]))
                {
                    index++;
                }

                if (index >= value.Length)
                {
                    break;
                }

                var builder = new StringBuilder();
                var closing = value[index] == '[' ? ']' : value[index] == '"' ? '"' : '\0';
                if (closing != '\0')
                {
                    index++;
                    var closed = false;
                    while (index < value.Length)
                    {
                        var current = value[index++];
                        if (current == closing)
                        {
                            if (index < value.Length && value[index] == closing)
                            {
                                builder.Append(closing);
                                index++;
                            }
                            else
                            {
                                closed = true;
                                break;
                            }
                        }
                        else
                        {
                            builder.Append(current);
                        }
                    }

                    if (!closed)
                    {
                        throw InvalidIdentifier(value, path);
                    }
                }
                else
                {
                    while (index < value.Length && value[index] != '.')
                    {
                        builder.Append(value[index++]);
                    }
                }

                if (builder.Length == 0 || (index < value.Length && value[index] != '.' && !char.IsWhiteSpace(value[index])))
                {
                    throw InvalidIdentifier(value, path);
                }

                parts.Add(builder.ToString());
                while (index < value.Length && char.IsWhiteSpace(value[index]))
                {
                    index++;
                }

                if (index < value.Length && value[index++] != '.')
                {
                    throw InvalidIdentifier(value, path);
                }
            }

            if (parts.Count != 3 || parts.Any(string.IsNullOrWhiteSpace))
            {
                throw InvalidIdentifier(value, path);
            }

            return parts.ToArray();
        }

        private static InvalidOperationException InvalidIdentifier(string value, string path)
            => new($"Invalid SRD0097 exception column identifier '{value}' in '{path}'. Use an exact [schema].[table].[column] name.");

        private static string GetObjectName(TSqlObject column)
            => string.Join(".", column.Name.Parts);

        private static Regex CreateLikeRegex(string pattern, string policyPath)
        {
            var expression = new StringBuilder("^");
            for (var index = 0; index < pattern.Length; index++)
            {
                var current = pattern[index];
                if (current == '%')
                {
                    expression.Append(".*");
                }
                else if (current == '_')
                {
                    expression.Append('.');
                }
                else if (current == '[')
                {
                    var end = pattern.IndexOf(']', index + 1);
                    if (end < 0 || end == index + 1)
                    {
                        throw new InvalidOperationException($"Invalid SQL LIKE pattern '{pattern}' in '{policyPath}'.");
                    }

                    var content = pattern.Substring(index + 1, end - index - 1);
                    var negate = content[0] == '^';
                    var contentStart = negate ? 1 : 0;
                    if (contentStart == content.Length)
                    {
                        throw new InvalidOperationException($"Invalid SQL LIKE pattern '{pattern}' in '{policyPath}'.");
                    }

                    expression.Append('[');
                    if (negate)
                    {
                        expression.Append('^');
                    }

                    for (var charIndex = contentStart; charIndex < content.Length; charIndex++)
                    {
                        var value = content[charIndex];
                        if (value is '\\' or ']' or '^')
                        {
                            expression.Append('\\');
                        }

                        expression.Append(value);
                    }

                    expression.Append(']');
                    index = end;
                }
                else
                {
                    expression.Append(Regex.Escape(current.ToString()));
                }
            }

            expression.Append('$');
            try
            {
                return new Regex(
                    expression.ToString(),
                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Singleline,
                    TimeSpan.FromMilliseconds(100));
            }
            catch (ArgumentException exception)
            {
                throw new InvalidOperationException($"Invalid SQL LIKE pattern '{pattern}' in '{policyPath}'.", exception);
            }
        }

        private static bool IsNumeric(TSqlObject column)
        {
            var numericTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "bigint", "bit", "decimal", "float", "int", "money", "numeric", "real", "smallint", "smallmoney", "tinyint",
            };
            var pending = new Stack<TSqlObject>(column.GetReferenced(Column.DataType, DacQueryScopes.All));
            var visited = new HashSet<TSqlObject>();

            while (pending.Count > 0)
            {
                var dataType = pending.Pop();
                if (!visited.Add(dataType))
                {
                    continue;
                }

                if (numericTypes.Contains(dataType.Name.Parts.Last()))
                {
                    return true;
                }

                foreach (var referenced in dataType.GetReferenced(DacQueryScopes.All))
                {
                    pending.Push(referenced);
                }
            }

            return false;
        }

        private static T ReadJson<T>(string path)
        {
            try
            {
                using var stream = File.OpenRead(path);
                var serializer = new DataContractJsonSerializer(
                    typeof(T),
                    new DataContractJsonSerializerSettings { UseSimpleDictionaryFormat = true });
                return (T)serializer.ReadObject(stream)!;
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or SerializationException or ArgumentException)
            {
                throw new InvalidOperationException($"Unable to read SRD0097 configuration file '{path}': {exception.Message}", exception);
            }
        }

        [DataContract]
        private sealed class PolicyDocument
        {
            [DataMember(Name = "Version", IsRequired = true)]
            public string? Version { get; set; }

            [DataMember(Name = "Labels", IsRequired = true)]
            public Dictionary<string, PolicyLabel>? Labels { get; set; }

            [DataMember(Name = "InformationTypes", IsRequired = true)]
            public Dictionary<string, PolicyInformationType>? InformationTypes { get; set; }
        }

        [DataContract]
        private sealed class PolicyLabel
        {
            [DataMember(Name = "DisplayName")]
            public string? DisplayName { get; set; }

            [DataMember(Name = "Rank")]
            public string? Rank { get; set; }

            [DataMember(Name = "Enabled", IsRequired = true)]
            public bool Enabled { get; set; }
        }

        [DataContract]
        private sealed class PolicyInformationType
        {
            [DataMember(Name = "DisplayName")]
            public string? DisplayName { get; set; }

            [DataMember(Name = "Order", IsRequired = true)]
            public int Order { get; set; }

            [DataMember(Name = "RecommendedLabelId", IsRequired = true)]
            public string? RecommendedLabelId { get; set; }

            [DataMember(Name = "Enabled", IsRequired = true)]
            public bool Enabled { get; set; }

            [DataMember(Name = "Keywords")]
            public List<PolicyKeyword>? Keywords { get; set; }
        }

        [DataContract]
        private sealed class PolicyKeyword
        {
            [DataMember(Name = "Pattern")]
            public string? Pattern { get; set; }

            [DataMember(Name = "CanBeNumeric", IsRequired = true)]
            public bool CanBeNumeric { get; set; }

            [DataMember(Name = "Excluded")]
            public bool? Excluded { get; set; }
        }

        [DataContract]
        private sealed class ExceptionDocument
        {
            [DataMember(Name = "exceptions")]
            public List<PolicyExceptionEntry>? Exceptions { get; set; }
        }

        [DataContract]
        private sealed class PolicyExceptionEntry
        {
            [DataMember(Name = "column")]
            public string? Column { get; set; }

            [DataMember(Name = "reason")]
            public string? Reason { get; set; }

            [DataMember(Name = "approvedBy")]
            public string? ApprovedBy { get; set; }

            [DataMember(Name = "expires")]
            public string? Expires { get; set; }
        }

        private sealed class PolicyException
        {
            public PolicyException(string reason, string? approvedBy, DateTime? expires)
            {
                Reason = reason;
                ApprovedBy = approvedBy;
                Expires = expires;
            }

            public string Reason { get; }

            public string? ApprovedBy { get; }

            public DateTime? Expires { get; }
        }

        internal sealed class PolicyMatch
        {
            public PolicyMatch(string informationType, string pattern, bool canBeNumeric, Regex regex)
            {
                InformationType = informationType;
                Pattern = pattern;
                CanBeNumeric = canBeNumeric;
                Regex = regex;
            }

            public string InformationType { get; }

            public string Pattern { get; }

            public bool CanBeNumeric { get; }

            public Regex Regex { get; }
        }
    }
}
