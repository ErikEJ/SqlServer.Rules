using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using EditorConfig.Core;
using Microsoft.SqlServer.Dac.Model;

namespace SqlServer.Rules.Design
{
    internal sealed class SensitivityPolicy
    {
        private static readonly StringComparer IdentifierComparer = StringComparer.InvariantCultureIgnoreCase;
        private static readonly HashSet<string> NumericTypeNames = new(StringComparer.OrdinalIgnoreCase)
        {
            "bigint", "bit", "decimal", "float", "int", "money", "numeric", "real", "smallint", "smallmoney", "tinyint",
        };

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

        public static SensitivityPolicy Load(string policyPath, string? exceptionsPath)
        {
            var document = ReadJson<PolicyDocument>(policyPath, ValidatePolicyJson);
            if (!string.Equals(document.Version, "2.0", StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Unsupported SRD0097 policy version '{document.Version}' in '{policyPath}'. Only version 2.0 is supported.");
            }

            if (document.Labels == null || document.InformationTypes == null)
            {
                throw new InvalidOperationException($"SRD0097 policy '{policyPath}' must contain Labels and InformationTypes.");
            }

            var labels = new Dictionary<string, PolicyLabel>(document.Labels, StringComparer.OrdinalIgnoreCase);
            var matches = new List<PolicyMatch>();
            foreach (var item in document.InformationTypes
                .OrderBy(x => x.Value.Order)
                .ThenBy(x => x.Value.DisplayName, StringComparer.Ordinal)
                .ThenBy(x => x.Key, StringComparer.Ordinal))
            {
                var informationType = item.Value;
                if (string.IsNullOrWhiteSpace(informationType.RecommendedLabelId)
                    || !labels.TryGetValue(informationType.RecommendedLabelId!, out var label))
                {
                    throw new InvalidOperationException($"Information type '{informationType.DisplayName}' in '{policyPath}' references an unknown RecommendedLabelId.");
                }

                if (!informationType.Enabled)
                {
                    continue;
                }

                if (!label.Enabled
                    || string.Equals(label.DisplayName, "Public", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                foreach (var keyword in informationType.Keywords ?? new List<PolicyKeyword>())
                {
                    if (keyword.Pattern == null)
                    {
                        throw new InvalidOperationException($"Information type '{informationType.DisplayName}' in '{policyPath}' contains a keyword without a Pattern.");
                    }

                    var regex = CreateLikeRegex(keyword.Pattern, policyPath);
                    if (keyword.Excluded != true)
                    {
                        matches.Add(new PolicyMatch(
                        informationType.DisplayName ?? informationType.RecommendedLabelId!,
                        keyword.Pattern,
                        keyword.CanBeNumeric,
                        regex));
                    }
                }
            }

            var exceptions = exceptionsPath == null
                ? new Dictionary<string, PolicyException>(IdentifierComparer)
                : LoadExceptions(exceptionsPath);

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

        public bool IsExcluded(TSqlObject column, DateTime utcNow)
            => exceptions.TryGetValue(GetObjectName(column), out var exception)
                && (exception.Expires == null || utcNow.Date <= exception.Expires.Value);

        public static string GetCacheKey(string policyPath, string? exceptionsPath)
            => policyPath + "\0" + exceptionsPath;

        public static (long PolicyTicks, long PolicyLength, long ExceptionsTicks, long ExceptionsLength) GetFileStamp(
            string policyPath,
            string? exceptionsPath)
        {
            var policy = new FileInfo(policyPath);
            policy.Refresh();
            var exceptions = exceptionsPath == null ? null : new FileInfo(exceptionsPath);
            exceptions?.Refresh();
            return (
                policy.LastWriteTimeUtc.Ticks,
                policy.Exists ? policy.Length : 0,
                exceptions?.LastWriteTimeUtc.Ticks ?? 0,
                exceptions?.Exists == true ? exceptions.Length : 0);
        }

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

        private static Dictionary<string, PolicyException> LoadExceptions(string path)
        {
            var document = ReadJson<ExceptionDocument>(path, ValidateExceptionJson);
            var result = new Dictionary<string, PolicyException>(IdentifierComparer);
            var allEntries = new Dictionary<string, PolicyException>(IdentifierComparer);

            foreach (var exception in document.Exceptions ?? new List<PolicyExceptionEntry>())
            {
                if (string.IsNullOrWhiteSpace(exception.Column) || string.IsNullOrWhiteSpace(exception.Reason))
                {
                    throw new InvalidOperationException($"Each SRD0097 exception in '{path}' must specify a column and a non-empty reason.");
                }

                var name = ParseObjectName(exception.Column!, path);
                DateTime? expires = null;
                if (exception.Expires != null)
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

                var normalizedName = GetObjectKey(name);
                var entry = new PolicyException(exception.Reason!, exception.ApprovedBy, expires);

                if (allEntries.TryGetValue(normalizedName, out var existing)
                    && (existing.Reason != entry.Reason
                        || existing.ApprovedBy != entry.ApprovedBy
                        || existing.Expires != entry.Expires))
                {
                    throw new InvalidOperationException($"Conflicting SRD0097 exceptions for '{exception.Column}' in '{path}'.");
                }

                allEntries[normalizedName] = entry;
                result[normalizedName] = entry;
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

                    var unquoted = builder.ToString().Trim();
                    if (!Regex.IsMatch(unquoted, @"^[\p{L}_@#][\p{L}\p{Nd}_@$#]*$", RegexOptions.CultureInvariant))
                    {
                        throw InvalidIdentifier(value, path);
                    }

                    builder.Clear();
                    builder.Append(unquoted);
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

                if (index < value.Length)
                {
                    if (value[index++] != '.')
                    {
                        throw InvalidIdentifier(value, path);
                    }

                    while (index < value.Length && char.IsWhiteSpace(value[index]))
                    {
                        index++;
                    }

                    if (index == value.Length)
                    {
                        throw InvalidIdentifier(value, path);
                    }
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
            => GetObjectKey(column.Name.Parts);

        private static string GetObjectKey(IEnumerable<string> parts)
            => string.Join("\0", parts);

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
                    if (index + 1 == pattern.Length)
                    {
                        throw new InvalidOperationException($"Invalid SQL LIKE pattern '{pattern}' in '{policyPath}'.");
                    }

                    var end = pattern[index + 1] == ']'
                        ? pattern.IndexOf(']', index + 2)
                        : pattern.IndexOf(']', index + 1);
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
                        if (value is '\\' or '[' or ']' or '^')
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
            var pending = new Stack<TSqlObject>(column.GetReferenced(Column.DataType, DacQueryScopes.All));
            var visited = new HashSet<TSqlObject>();

            while (pending.Count > 0)
            {
                var dataType = pending.Pop();
                if (!visited.Add(dataType))
                {
                    continue;
                }

                if (NumericTypeNames.Contains(dataType.Name.Parts.Last()))
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

        private static T ReadJson<T>(string path, Action<byte[], string> validateJson)
        {
            try
            {
                var json = File.ReadAllBytes(path);
                validateJson(json, path);
                using var stream = new MemoryStream(json);
                var serializer = new DataContractJsonSerializer(
                    typeof(T),
                    new DataContractJsonSerializerSettings { UseSimpleDictionaryFormat = true });
                return serializer.ReadObject(stream) is T value
                    ? value
                    : throw new SerializationException($"Configuration file '{path}' must contain a JSON object.");
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or SerializationException or XmlException or ArgumentException)
            {
                throw new InvalidOperationException($"Unable to read SRD0097 configuration file '{path}': {exception.Message}", exception);
            }
        }

        private static void ValidatePolicyJson(byte[] json, string path)
        {
            var root = ReadJsonXml(json, path);
            ValidateObjectMembers(root, "policy", path, "LastModifiedUtc", "Version", "Labels", "InformationTypes");

            var labels = GetProperty(root, "Labels");
            if (labels != null)
            {
                ValidateObject(labels, "Labels", path);
                foreach (XmlElement label in labels.ChildNodes)
                {
                    ValidateObjectMembers(label, $"label '{label.LocalName}'", path, "DisplayName", "Description", "Rank", "Order", "Enabled");
                }
            }

            var informationTypes = GetProperty(root, "InformationTypes");
            if (informationTypes != null)
            {
                ValidateObject(informationTypes, "InformationTypes", path);
                foreach (XmlElement informationType in informationTypes.ChildNodes)
                {
                    var context = $"information type '{informationType.LocalName}'";
                    ValidateObjectMembers(
                        informationType,
                        context,
                        path,
                        "DisplayName",
                        "Description",
                        "Order",
                        "RecommendedLabelId",
                        "Enabled",
                        "Custom",
                        "Keywords");

                    var keywords = GetProperty(informationType, "Keywords");
                    if (keywords != null && !string.Equals(keywords.GetAttribute("type"), "null", StringComparison.Ordinal))
                    {
                        ValidateArray(keywords, $"{context} keywords", path);
                        foreach (XmlElement keyword in keywords.ChildNodes)
                        {
                            ValidateObjectMembers(
                                keyword,
                                $"{context} keyword",
                                path,
                                "Pattern",
                                "CanBeNumeric",
                                "Excluded",
                                "Custom");
                        }
                    }
                }
            }
        }

        private static void ValidateExceptionJson(byte[] json, string path)
        {
            var root = ReadJsonXml(json, path);
            ValidateObjectMembers(root, "exceptions file", path, "exceptions");

            var exceptions = GetProperty(root, "exceptions");
            if (exceptions != null && !string.Equals(exceptions.GetAttribute("type"), "null", StringComparison.Ordinal))
            {
                ValidateArray(exceptions, "exceptions", path);
                foreach (XmlElement exception in exceptions.ChildNodes)
                {
                    ValidateObjectMembers(exception, "exception", path, "column", "reason", "approvedBy", "expires");
                }
            }
        }

        private static XmlElement ReadJsonXml(byte[] json, string path)
        {
            using var reader = JsonReaderWriterFactory.CreateJsonReader(json, XmlDictionaryReaderQuotas.Max);
            var document = new XmlDocument();
            document.Load(reader);
            var root = document.DocumentElement;
            if (root == null || !string.Equals(root.GetAttribute("type"), "object", StringComparison.Ordinal))
            {
                throw new SerializationException($"Configuration file '{path}' must contain a JSON object.");
            }

            return root;
        }

        private static void ValidateObjectMembers(XmlElement element, string context, string path, params string[] allowedMembers)
        {
            ValidateObject(element, context, path);
            var allowed = new HashSet<string>(allowedMembers, StringComparer.Ordinal);
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (XmlElement child in element.ChildNodes)
            {
                if (!allowed.Contains(child.LocalName))
                {
                    throw new SerializationException(
                        $"Configuration file '{path}' contains unsupported field '{child.LocalName}' in {context}.");
                }

                if (!seen.Add(child.LocalName))
                {
                    throw new SerializationException(
                        $"Configuration file '{path}' contains duplicate field '{child.LocalName}' in {context}.");
                }
            }
        }

        private static void ValidateObject(XmlElement element, string context, string path)
        {
            if (!string.Equals(element.GetAttribute("type"), "object", StringComparison.Ordinal))
            {
                throw new SerializationException($"Configuration file '{path}' must contain an object for {context}.");
            }
        }

        private static void ValidateArray(XmlElement element, string context, string path)
        {
            if (!string.Equals(element.GetAttribute("type"), "array", StringComparison.Ordinal))
            {
                throw new SerializationException($"Configuration file '{path}' must contain an array for {context}.");
            }

            foreach (XmlElement item in element.ChildNodes)
            {
                if (!string.Equals(item.LocalName, "item", StringComparison.Ordinal))
                {
                    throw new SerializationException($"Configuration file '{path}' contains an invalid array item in {context}.");
                }
            }
        }

        private static XmlElement? GetProperty(XmlElement element, string name)
        {
            foreach (XmlElement child in element.ChildNodes)
            {
                if (string.Equals(child.LocalName, name, StringComparison.Ordinal))
                {
                    return child;
                }
            }

            return null;
        }

        [DataContract]
        private sealed class PolicyDocument
        {
            [DataMember(Name = "LastModifiedUtc")]
            public string? LastModifiedUtc { get; set; }

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

            [DataMember(Name = "Description")]
            public string? Description { get; set; }

            [DataMember(Name = "Rank")]
            public string? Rank { get; set; }

            [DataMember(Name = "Order")]
            public int Order { get; set; }

            [DataMember(Name = "Enabled", IsRequired = true)]
            public bool Enabled { get; set; }
        }

        [DataContract]
        private sealed class PolicyInformationType
        {
            [DataMember(Name = "DisplayName")]
            public string? DisplayName { get; set; }

            [DataMember(Name = "Description")]
            public string? Description { get; set; }

            [DataMember(Name = "Order", IsRequired = true)]
            public int Order { get; set; }

            [DataMember(Name = "RecommendedLabelId", IsRequired = true)]
            public string? RecommendedLabelId { get; set; }

            [DataMember(Name = "Enabled", IsRequired = true)]
            public bool Enabled { get; set; }

            [DataMember(Name = "Custom")]
            public bool Custom { get; set; }

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

            [DataMember(Name = "Custom")]
            public bool Custom { get; set; }
        }

        [DataContract]
        private sealed class ExceptionDocument
        {
            [DataMember(Name = "exceptions", IsRequired = true)]
            public List<PolicyExceptionEntry>? Exceptions { get; set; }
        }

        [DataContract]
        private sealed class PolicyExceptionEntry
        {
            [DataMember(Name = "column", IsRequired = true)]
            public string? Column { get; set; }

            [DataMember(Name = "reason", IsRequired = true)]
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
