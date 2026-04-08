using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;
using SqlServer.Rules.Globals;
using SqlServer.Rules.Naming;

namespace SqlServer.Rules.Performance
{
    /// <summary>
    /// General naming violation.
    /// </summary>
    /// <FriendlyName>Name standard</FriendlyName>
    /// <IsIgnorable>false</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// Multiple possible rule violations:
    /// - Name '{name}' starts with a number.
    /// - Name '{name}' contains invalid characters. Please only use alphanumerics and underscores.
    /// - Primary Key, Index, Foreign Key, Check Constraint, and Default Constraint names must match their configured SRN0007 regex.
    /// - Regex overrides can be configured in .editorconfig using `sqlserver_rules.srn0007.[pk|fk|ix|ux|ck|df]_regex`.
    /// - Supported regex tokens are `{{tableName}}`, `{{schemaName}}`, `{{foreignTableName}}`, `{{foreignSchemaName}}`, and `{{columnName}}`.
    /// - Default regex patterns:
    ///   - `pk_regex`: `^PK_{{tableName}}$`
    ///   - `ix_regex`: `^IX_{{tableName}}_.*`
    ///   - `ux_regex`: `^UX_{{tableName}}_.*`
    ///   - `fk_regex`: `^FK_{{tableName}}_{{foreignTableName}}.*`
    ///   - `ck_regex`: `^CK_{{tableName}}_.*`
    ///   - `df_regex`: `^DF_{{tableName}}_{{columnName}}$`
    /// </remarks>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    /// <seealso cref="SqlServer.Rules.Naming.NamingViolationRule" />
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Naming,
        RuleScope = SqlRuleScope.Element)]
    public sealed class GeneralNamingRules : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRN0007";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "General naming rules.";

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneralNamingRules"/> class.
        /// </summary>
        public GeneralNamingRules()
            : base(
            ModelSchema.Table,
            ModelSchema.View,
            ModelSchema.ScalarFunction,
            ModelSchema.TableValuedFunction,
            ModelSchema.Procedure,
            ModelSchema.PrimaryKeyConstraint,
            ModelSchema.Index,
            ModelSchema.ForeignKeyConstraint,
            ModelSchema.DefaultConstraint,
            ModelSchema.CheckConstraint,
            ModelSchema.DmlTrigger
        )
        {
        }

        /// <summary>
        /// Performs analysis and returns a list of problems detected
        /// </summary>
        /// <param name="ruleExecutionContext">Contains the schema model and model element to analyze</param>
        /// <returns>
        /// The problems detected by the rule in the given element
        /// </returns>
        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            var problems = new List<SqlRuleProblem>();
            var sqlObj = ruleExecutionContext.ModelElement;
            if (sqlObj == null || sqlObj.IsWhiteListed(ruleExecutionContext))
            {
                return problems;
            }

            var fragment = ruleExecutionContext.ScriptFragment?.GetFragment();

            var name = sqlObj.Name.Parts.LastOrDefault();
            var objectType = sqlObj.ObjectType.Name;
            var parentObj = sqlObj.GetParent(DacQueryScopes.All);

            if (string.IsNullOrWhiteSpace(name))
            {
                problems.Add(new SqlRuleProblem(MessageFormatter.FormatMessage($"{objectType} found without a name.", RuleId), parentObj));
                return problems;
            }

            if (Regex.IsMatch(name, @"^\d"))
            {
                problems.Add(new SqlRuleProblem(MessageFormatter.FormatMessage($"Name '{name}' starts with a number.", RuleId), sqlObj, fragment));
            }

            if (Regex.IsMatch(name, @"^[^A-z0-9_]*$"))
            {
                problems.Add(new SqlRuleProblem(MessageFormatter.FormatMessage($"Name '{name}' contains invalid characters. Please only use alphanumerics and underscores.", RuleId), sqlObj, fragment));
            }

            if (parentObj == null)
            {
                return problems;
            }

            var tableName = parentObj.Name.Parts.LastOrDefault();
            var schemaName = GetSchemaName(parentObj.Name);
            switch (objectType.ToUpperInvariant())
            {
                case "PRIMARYKEYCONSTRAINT":
                    var pkRegex = NamingRuleRegexConfiguration.GetConfiguredRegex(sqlObj, "pk_regex", "^PK_{{tableName}}$");
                    if (!IsNameMatch(name, pkRegex, tableName, schemaName, null, null, null, out var resolvedPkRegex))
                    {
                        problems.Add(new SqlRuleProblem(MessageFormatter.FormatMessage($"Primary Key '{name}' does not follow the company naming standard. Please match regex {resolvedPkRegex}.", RuleId), sqlObj, fragment));
                    }

                    break;
                case "INDEX":

                    if (fragment == null)
                    {
                        return problems;
                    }

                    var idx = fragment as CreateIndexStatement;

                    if (idx == null)
                    {
                        return problems;
                    }

                    var re = NamingRuleRegexConfiguration.GetConfiguredRegex(sqlObj, "ix_regex", "^IX_{{tableName}}_.*");
                    if (idx.Unique)
                    {
                        re = NamingRuleRegexConfiguration.GetConfiguredRegex(sqlObj, "ux_regex", "^UX_{{tableName}}_.*");
                    }

                    if (!IsNameMatch(name, re, tableName, schemaName, null, null, null, out var resolvedIndexRegex))
                    {
                        problems.Add(new SqlRuleProblem(MessageFormatter.FormatMessage($"Index '{name}' does not follow the company naming standard. Please match regex {resolvedIndexRegex}.", RuleId), sqlObj, fragment));
                    }

                    break;
                case "FOREIGNKEYCONSTRAINT":
                    // var fk = fragment as createke;
                    var tableFk = ruleExecutionContext.SchemaModel.GetObject(ForeignKeyConstraint.TypeClass, sqlObj.Name, DacQueryScopes.All);
                    var foreignTable = tableFk.GetReferencedRelationshipInstances(ForeignKeyConstraint.ForeignTable, DacQueryScopes.All)
                        .Select(x => x.ObjectName).ToList()
                        .FirstOrDefault();
                    var foreignTableName = foreignTable?.Parts.LastOrDefault();
                    var foreignSchemaName = GetSchemaName(foreignTable);
                    var fkRegex = NamingRuleRegexConfiguration.GetConfiguredRegex(sqlObj, "fk_regex", "^FK_{{tableName}}_{{foreignTableName}}.*");

                    if (!IsNameMatch(name, fkRegex, tableName, schemaName, foreignTableName, foreignSchemaName, null, out var resolvedFkRegex))
                    {
                        problems.Add(new SqlRuleProblem(MessageFormatter.FormatMessage($"Foreign Key '{name}' does not follow the company naming standard. Please match regex {resolvedFkRegex}.", RuleId), sqlObj, fragment));
                    }

                    break;
                case "CHECKCONSTRAINT":
                    var ckRegex = NamingRuleRegexConfiguration.GetConfiguredRegex(sqlObj, "ck_regex", "^CK_{{tableName}}_.*");
                    if (!IsNameMatch(name, ckRegex, tableName, schemaName, null, null, null, out var resolvedCkRegex))
                    {
                        problems.Add(new SqlRuleProblem(MessageFormatter.FormatMessage($"Check Constraint '{name}' does not follow the company naming standard. Please match regex {resolvedCkRegex}.", RuleId), sqlObj, fragment));
                    }

                    break;
                case "DEFAULTCONSTRAINT":
                    var columnName = GetReferencedName(sqlObj, DefaultConstraint.TargetColumn, "Column");
                    var dfRegex = NamingRuleRegexConfiguration.GetConfiguredRegex(sqlObj, "df_regex", "^DF_{{tableName}}_{{columnName}}$");

                    if (!IsNameMatch(name, dfRegex, tableName, schemaName, null, null, columnName, out var resolvedDfRegex))
                    {
                        problems.Add(new SqlRuleProblem(MessageFormatter.FormatMessage($"Constraint '{name}' does not follow the company naming standard. Please match regex {resolvedDfRegex}.", RuleId), sqlObj, fragment));
                    }

                    // ADD OTHER TYPES IF DESIRED IF YOU WANT THEM TO MATCH A SPECIFIC FORMAT
                    break;
            }

            return problems;
        }

        private static string GetReferencedName(TSqlObject sqlObj, ModelRelationshipClass relation = null, string typeToLookFor = "Table")
        {
            var referenced = sqlObj.GetReferenced().FirstOrDefault(o => Comparer.Equals(o.ObjectType.Name, typeToLookFor));

            if (referenced == null)
            {
                return null;
            }

            return referenced.Name.Parts.LastOrDefault();
        }

        private static bool IsNameMatch(
            string objectName,
            string pattern,
            string tableName,
            string schemaName,
            string foreignTableName,
            string foreignSchemaName,
            string columnName,
            out string resolvedPattern)
        {
            resolvedPattern = ResolveRegexPattern(pattern, tableName, schemaName, foreignTableName, foreignSchemaName, columnName);

            try
            {
                return Regex.IsMatch(objectName, resolvedPattern, RegexOptions.IgnoreCase);
            }
            catch (ArgumentException)
            {
                resolvedPattern = $"'{resolvedPattern}' (invalid regex)";
                return false;
            }
        }

        private static string ResolveRegexPattern(
            string pattern,
            string tableName,
            string schemaName,
            string foreignTableName,
            string foreignSchemaName,
            string columnName)
        {
            var resolvedPattern = pattern;
            resolvedPattern = ReplaceToken(resolvedPattern, "tableName", tableName);
            resolvedPattern = ReplaceToken(resolvedPattern, "schemaName", schemaName);
            resolvedPattern = ReplaceToken(resolvedPattern, "foreignTableName", foreignTableName);
            resolvedPattern = ReplaceToken(resolvedPattern, "foreignSchemaName", foreignSchemaName);
            resolvedPattern = ReplaceToken(resolvedPattern, "columnName", columnName);

            return resolvedPattern;
        }

        private static string ReplaceToken(string pattern, string tokenName, string value)
        {
            var replacement = string.IsNullOrEmpty(value)
                ? string.Empty
                : Regex.Escape(value);
            var doubleBracesPattern = @"\{\{\s*" + Regex.Escape(tokenName) + @"\s*\}\}";
            return Regex.Replace(pattern, doubleBracesPattern, replacement, RegexOptions.IgnoreCase);
        }

        private static string GetSchemaName(ObjectIdentifier objectName)
        {
            if (objectName == null || objectName.Parts.Count < 2)
            {
                return string.Empty;
            }

            return objectName.Parts[objectName.Parts.Count - 2];
        }
    }
}
