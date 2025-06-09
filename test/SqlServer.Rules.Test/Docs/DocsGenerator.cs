using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using LoxSmoke.DocXml;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.Rules.Design;
using TSQLSmellSCA;

namespace SqlServer.Rules.Tests.Docs;

[TestClass]
[TestCategory("Docs")]
public class DocsGenerator
{
    private static readonly Dictionary<string, (string, string, string)> MicrosoftRules = new()
    {
        {
            "SR0001",  ("https://learn.microsoft.com/sql/tools/sql-database-projects/concepts/sql-code-analysis/t-sql-design-issues#sr0001-avoid-select--in-stored-procedures-views-and-table-valued-functions",
            "Avoid SELECT * in stored procedures, views, and table-valued functions", "Design")
        },
        {
            "SR0008",  ("https://learn.microsoft.com/sql/tools/sql-database-projects/concepts/sql-code-analysis/t-sql-design-issues#sr0008-consider-using-scope_identity-instead-of-identity",
            "Consider using SCOPE_IDENTITY instead of @@IDENTITY", "Design")
        },
        {
            "SR0009",  ("https://learn.microsoft.com/sql/tools/sql-database-projects/concepts/sql-code-analysis/t-sql-design-issues#sr0009-avoid-using-types-of-variable-length-that-are-size-1-or-2",
            "Avoid using types of variable length that are size 1 or 2", "Design")
        },
        {
            "SR0010",  ("https://learn.microsoft.com/sql/tools/sql-database-projects/concepts/sql-code-analysis/t-sql-design-issues#sr0010-avoid-using-deprecated-syntax-when-you-join-tables-or-views",
            "Avoid using deprecated syntax when you join tables or views", "Design")
        },
        {
            "SR0013",  ("https://learn.microsoft.com/sql/tools/sql-database-projects/concepts/sql-code-analysis/t-sql-design-issues#sr0013-output-parameter-parameter-isnt-populated-in-all-code-paths",
            "Output parameter (parameter) isn't populated in all code paths", "Design")
        },
        {
            "SR0014",  ("https://learn.microsoft.com/sql/tools/sql-database-projects/concepts/sql-code-analysis/t-sql-design-issues#sr0014-data-loss-might-occur-when-casting-from-type1-to-type2",
            "Data loss might occur when casting from {Type1} to {Type2}", "Design")
        },
        {
            "SR0011",  ("https://learn.microsoft.com/sql/tools/sql-database-projects/concepts/sql-code-analysis/t-sql-naming-issues#sr0011-avoid-using-special-characters-in-object-names",
            "Avoid using special characters in object names", "Naming")
        },
        {
            "SR0012",  ("https://learn.microsoft.com/sql/tools/sql-database-projects/concepts/sql-code-analysis/t-sql-naming-issues#sr0012-avoid-using-reserved-words-for-type-names",
            "Avoid using reserved words for type names", "Naming")
        },
        {
            "SR0016",  ("https://learn.microsoft.com/sql/tools/sql-database-projects/concepts/sql-code-analysis/t-sql-naming-issues#sr0016-avoid-using-sp_-as-a-prefix-for-stored-procedures",
            "Avoid using sp_ as a prefix for stored procedures", "Naming")
        },
        {
            "SR0004",  ("https://learn.microsoft.com/sql/tools/sql-database-projects/concepts/sql-code-analysis/t-sql-performance-issues#sr0004-avoid-using-columns-that-dont-have-indexes-as-test-expressions-in-in-predicates",
            "Avoid using columns that don't have indexes as test expressions in IN predicates", "Performance")
        },
        {
            "SR0005",  ("https://learn.microsoft.com/sql/tools/sql-database-projects/concepts/sql-code-analysis/t-sql-performance-issues#sr0005-avoid-using-patterns-that-start-with--in-like-predicates",
            "Avoid using patterns that start with \"%\" in LIKE predicates", "Performance")
        },
        {
            "SR0006",  ("https://learn.microsoft.com/sql/tools/sql-database-projects/concepts/sql-code-analysis/t-sql-performance-issues#sr0006-move-a-column-reference-to-one-side-of-a-comparison-operator-to-use-a-column-index",
            "Move a column reference to one side of a comparison operator to use a column index", "Performance")
        },
        {
            "SR0007",  ("https://learn.microsoft.com/sql/tools/sql-database-projects/concepts/sql-code-analysis/t-sql-performance-issues#sr0007-use-isnullcolumn-default_value-on-nullable-columns-in-expressions",
            "Use ISNULL(column, default_value) on nullable columns in expressions", "Performance")
        },
        {
            "SR0015",  ("https://learn.microsoft.com/sql/tools/sql-database-projects/concepts/sql-code-analysis/t-sql-performance-issues#sr0015-extract-deterministic-function-calls-from-where-predicates",
            "Extract deterministic function calls from WHERE predicates", "Performance")
        },
    };

    [TestMethod]
    public void GenerateDocs()
    {
        var assembly = typeof(ObjectCreatedWithInvalidOptionsRule).Assembly;
        var assemblyPath = assembly.Location;
        const string docsFolder = "../../../../../docs";
        const string rulesScriptFolder = "../../../../../sqlprojects/TSQLSmellsTest";

        var rules = assembly.GetTypes()
            .Where(t => t.IsClass
                        && !t.IsAbstract
                        && t.IsSubclassOf(typeof(BaseSqlCodeAnalysisRule))
                        && t.GetCustomAttributes(typeof(ExportCodeAnalysisRuleAttribute), false).Any())
            .ToList();

        var ruleScripts = CollectRuleScripts(rulesScriptFolder);

        var smellsAssembly = typeof(Smells).Assembly;

        var tSqlSmellRules = smellsAssembly.GetTypes()
            .Where(t => t.IsClass
                        && !t.IsAbstract
                        && t.GetCustomAttributes(typeof(ExportCodeAnalysisRuleAttribute), false).Any())
            .ToList();

        rules.AddRange(tSqlSmellRules);

        var categories = rules.Select(t =>
        {
            var ruleAttribute = t.GetCustomAttributes(typeof(ExportCodeAnalysisRuleAttribute), false).FirstOrDefault() as ExportCodeAnalysisRuleAttribute;
            return ruleAttribute!.Category;
        }).Distinct().Order().ToList();

        CreateFolders(docsFolder, categories);

        var xmlPath = assemblyPath.Replace(".dll", ".xml", StringComparison.OrdinalIgnoreCase);
        Assert.IsTrue(File.Exists(xmlPath));
        var reader = new DocXmlReader(xmlPath);

        rules.ForEach(t =>
        {
            var comments = reader.GetTypeComments(t);
            var ruleAttribute = t.GetCustomAttributes(typeof(ExportCodeAnalysisRuleAttribute), false).FirstOrDefault() as ExportCodeAnalysisRuleAttribute;

            var elements = GetRuleElements(t, ruleAttribute);

            GenerateRuleMarkdown(comments, elements, ruleScripts, ruleAttribute, Path.Combine(docsFolder, ruleAttribute!.Category), t.Assembly.GetName().Name, t.Namespace, t.Name);
        });

        GenerateTocMarkdown(rules, categories, ruleScripts, reader, docsFolder);
    }

    private static void CreateFolders(string docsFolder, List<string> categories)
    {
        if (!Directory.Exists(docsFolder))
        {
            Directory.CreateDirectory(docsFolder);
        }

        foreach (var category in categories)
        {
            var categoryFolder = Path.Combine(docsFolder, category);
            if (!Directory.Exists(categoryFolder))
            {
                Directory.CreateDirectory(categoryFolder);
            }
        }
    }

    private static List<string> GetRuleElements(Type type, ExportCodeAnalysisRuleAttribute attribute)
    {
        var elements = new List<string>();

        if (attribute.RuleScope == SqlRuleScope.Element)
        {
            var constructor = type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);

            if (constructor != null)
            {
                var instance = (BaseSqlCodeAnalysisRule)constructor.Invoke(null);
                if (instance != null)
                {
                    foreach (var item in instance.SupportedElementTypes)
                    {
                        elements.Add(item.Name.ToSentence());
                    }
                }
            }
        }
        else
        {
            elements.Add("Model");
        }

        return elements.Order().ToList();
    }

    private static void GenerateRuleMarkdown(TypeComments comments, List<string> elements, Dictionary<string, List<string>> ruleScripts, ExportCodeAnalysisRuleAttribute attribute, string docsFolder, string assemblyName, string nameSpace, string className)
    {
        var isIgnorable = string.Empty;
        var friendlyName = string.Empty;
        var exampleMd = string.Empty;

        if (comments?.FullCommentText != null)
        {
            var fullComments = LoadXml(comments);

            isIgnorable = fullComments.SelectSingleNode("comments/IsIgnorable")?.InnerText ?? "false";
            friendlyName = fullComments.SelectSingleNode("comments/FriendlyName")?.InnerText;
            exampleMd = fullComments.SelectSingleNode("comments/ExampleMd")?.InnerText;
        }

        if (string.IsNullOrWhiteSpace(friendlyName))
        {
            friendlyName = className.ToSentence();
        }

        if (attribute.Id.StartsWith("Smells.", StringComparison.OrdinalIgnoreCase))
        {
            friendlyName = attribute.Description;
            isIgnorable = "false";
        }

        var stringBuilder = new StringBuilder();

        var spaces = "  ";
        stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"# SQL Server Rule: {attribute.Id.ToId()}");
        stringBuilder.AppendLine(spaces);
        stringBuilder.AppendLine("|    |    |");
        stringBuilder.AppendLine("|----|----|");
        stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"| Assembly | {assemblyName} |");
        stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"| Namespace | {nameSpace} |");
        stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"| Class | {className} |");
        stringBuilder.AppendLine(spaces);
        stringBuilder.AppendLine("## Rule Information");
        stringBuilder.AppendLine(spaces);
        stringBuilder.AppendLine("|    |    |");
        stringBuilder.AppendLine("|----|----|");
        stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"| Id | {attribute.Id.ToId()} |");
        stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"| Friendly Name | {friendlyName} |");
        stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"| Category | {attribute.Category} |");
        stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"| Ignorable | {isIgnorable} |");
        stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"| Applicable Types | {elements.First()}  |");

        if (elements.Count > 1)
        {
            elements.RemoveAt(0);

            foreach (var element in elements)
            {
                stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"|   | {element} |");
            }
        }

        stringBuilder.AppendLine(spaces);
        stringBuilder.AppendLine("## Description");
        stringBuilder.AppendLine(spaces);
        stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"{attribute.Description}");

        if (!string.IsNullOrWhiteSpace(comments?.Summary))
        {
            stringBuilder.AppendLine(spaces);
            stringBuilder.AppendLine("## Summary");
            stringBuilder.AppendLine(spaces);
            stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"{TrimLeadingWhitespace(comments.Summary)}");
        }

        var scriptExamples = ruleScripts.ContainsKey(attribute.Id.ToId()) ? ruleScripts[attribute.Id.ToId()] : [];

        if (!string.IsNullOrWhiteSpace(exampleMd)
            || scriptExamples.Any())
        {
            stringBuilder.AppendLine(spaces);
            stringBuilder.AppendLine("### Examples");
        }

        if (!string.IsNullOrWhiteSpace(exampleMd))
        {
            stringBuilder.AppendLine(spaces);
            stringBuilder.Append(CultureInfo.InvariantCulture, $"{TrimLeadingWhitespace(exampleMd)}");
        }

        if (scriptExamples.Any())
        {
            stringBuilder.AppendLine(spaces);
            foreach (var script in scriptExamples)
            {
                stringBuilder.AppendLine("```sql");
                stringBuilder.AppendLine(script.Trim(Environment.NewLine.ToCharArray()).Trim());
                stringBuilder.AppendLine("```");
            }
        }

        if (!string.IsNullOrWhiteSpace(comments?.Remarks))
        {
            stringBuilder.AppendLine(spaces);
            stringBuilder.AppendLine("### Remarks");
            stringBuilder.AppendLine(spaces);
            stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"{comments.Remarks}");
        }

        stringBuilder.AppendLine(spaces);
        stringBuilder.AppendLine("<sub><sup>Generated by a tool</sup></sub>");

        var filePath = Path.Combine(docsFolder, $"{attribute.Id.ToId()}.md");
        File.WriteAllText(filePath, stringBuilder.ToString(), Encoding.UTF8);
    }

    private static string TrimLeadingWhitespace(string summary)
    {
        if (string.IsNullOrWhiteSpace(summary))
        {
            return string.Empty;
        }

        var lines = summary.Split([Environment.NewLine], StringSplitOptions.None);
        var trimmedLines = lines.Select(l => l.TrimStart()).ToArray();
        return string.Join(Environment.NewLine, trimmedLines);
    }

    private static void GenerateTocMarkdown(List<Type> sqlServerRules, List<string> categories, Dictionary<string, List<string>> ruleScripts, DocXmlReader reader, string docsFolder)
    {
        const string spaces = "  ";

        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine(spaces);
        stringBuilder.AppendLine("# Rules listed by namespace");
        stringBuilder.AppendLine(spaces);

        foreach (var category in categories)
        {
            stringBuilder.AppendLine(spaces);
            stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"## {category}");
            stringBuilder.AppendLine(spaces);

            stringBuilder.AppendLine("| Rule Id | Friendly Name | Ignorable | Description | Example? |");
            stringBuilder.AppendLine("|----|----|----|----|----|");
            var categoryRules = sqlServerRules
                .Where(t => ((ExportCodeAnalysisRuleAttribute)t.GetCustomAttributes(typeof(ExportCodeAnalysisRuleAttribute), false).FirstOrDefault())!.Category == category)
                .OrderBy(t => ((ExportCodeAnalysisRuleAttribute)t.GetCustomAttributes(typeof(ExportCodeAnalysisRuleAttribute), false).FirstOrDefault())!.Id)
                .ToList();
            foreach (var rule in categoryRules)
            {
                var comments = reader.GetTypeComments(rule);

                var isIgnorable = "No";
                var friendlyName = string.Empty;
                var exampleMd = string.Empty;

                if (comments?.FullCommentText != null)
                {
                    var fullComments = LoadXml(comments);

                    isIgnorable = fullComments.SelectSingleNode("comments/IsIgnorable")?.InnerText ?? "No";
                    friendlyName = fullComments.SelectSingleNode("comments/FriendlyName")?.InnerText;
                    exampleMd = fullComments.SelectSingleNode("comments/ExampleMd")?.InnerText;
                }

                if (string.IsNullOrWhiteSpace(friendlyName))
                {
                    friendlyName = rule.Name.ToSentence();
                }

                friendlyName = friendlyName.Replace("|", "&#124;", StringComparison.OrdinalIgnoreCase);

                isIgnorable = isIgnorable != "false" ? "Yes" : " ";

                exampleMd = string.IsNullOrWhiteSpace(exampleMd) ? " " : "Yes";

                var ruleAttribute = rule.GetCustomAttributes<ExportCodeAnalysisRuleAttribute>(false).First();

                if (ruleAttribute != null)
                {
                    if (ruleAttribute.Id.StartsWith("Smells.", StringComparison.OrdinalIgnoreCase))
                    {
                        friendlyName = ruleAttribute.Description;
                        isIgnorable = " ";
                    }

                    if (exampleMd == " ")
                    {
                        exampleMd = ruleScripts.Any(x => x.Key.Contains(ruleAttribute.Id.ToId(), StringComparison.OrdinalIgnoreCase)) ? "Yes" : " ";
                    }

                    var ruleLink = string.Empty;

                    ruleLink = $"[{ruleAttribute.Id.ToId()}]({category}/{ruleAttribute.Id.ToId()}.md)";

                    stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"| {ruleLink} | {friendlyName} | {isIgnorable} | {ruleAttribute!.Description?.Replace("|", "&#124;", StringComparison.OrdinalIgnoreCase)} | {exampleMd} |");
                }
            }
        }

        stringBuilder.AppendLine(spaces);
        stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"## Microsoft ");
        stringBuilder.AppendLine(spaces);

        stringBuilder.AppendLine("| Rule Id | Friendly Name | Ignorable | Description | Example? |");
        stringBuilder.AppendLine("|----|----|----|----|----|");

        foreach (var rule in MicrosoftRules)
        {
            stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"| [{rule.Key} {rule.Value.Item3} ]({rule.Value.Item1}) | {rule.Value.Item2} |  | {rule.Value.Item2} | Yes |");
        }

        stringBuilder.AppendLine(spaces);
        stringBuilder.AppendLine("<sub><sup>Generated by a tool</sup></sub>");

        File.WriteAllText(Path.Combine(docsFolder, "readme.md"), stringBuilder.ToString(), Encoding.UTF8);
    }

    private static XmlDocument LoadXml(TypeComments comments)
    {
        var fullXml = "<comments>" + comments.FullCommentText.Trim() + "</comments>";
        var fullComments = new XmlDocument();
        fullComments.LoadXml(fullXml);
        return fullComments;
    }

    private static Dictionary<string, List<string>> CollectRuleScripts(string rulesScriptFolder)
    {
        var ruleScripts = new Dictionary<string, List<string>>();
        var files = Directory.GetFiles(rulesScriptFolder, "*.sql", SearchOption.AllDirectories).ToList();
        foreach (var file in files)
        {
            var ruleLine = File.ReadAllLines(file).FirstOrDefault(l => l.StartsWith("--", StringComparison.OrdinalIgnoreCase));

            if (ruleLine == null || string.IsNullOrWhiteSpace(ruleLine))
            {
                continue;
            }

            var ruleList = ruleLine.Replace("--", string.Empty, StringComparison.OrdinalIgnoreCase).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var rule in ruleList)
            {
                if (!ruleScripts.ContainsKey(rule))
                {
                    ruleScripts.Add(rule, []);
                }

                ruleScripts[rule].Add(File.ReadAllText(file));
            }
        }

        return ruleScripts;
    }
}
