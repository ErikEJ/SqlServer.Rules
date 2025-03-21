using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;
using SqlServer.Rules.Globals;

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
    ///   <list type="bullet">
    ///     <item> Name '{name}' starts with a number. </item>
    ///     <item> Name '{name}' contains invalid characters. Please only use alphanumerics and underscores. </item>
    ///     <item> Primary Key '{name}' does not follow the company naming standard. Please use the name PK_{tableName}. </item>
    ///     <item> Index '{name}' does not follow the company naming standard. Please use the name IX##_{tableName}. </item>
    ///     <item> Foreign Key '{name}' does not follow the company naming standard. Please use the format FK##_{tableName}. </item>
    ///     <item> Check Constraint '{name}' does not follow the company naming standard. Please use the format CK_*. </item>
    ///     <item> Constraint '{name}' does not follow the company naming standard. Please use the name DF_{tableName}_{columnName}. </item>
    ///   </list>
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
            if (sqlObj == null || sqlObj.IsWhiteListed())
            {
                return problems;
            }

            var fragment = ruleExecutionContext.ScriptFragment?.GetFragment();

            if (fragment == null)
            {
                return problems;
            }

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

            var tableName = parentObj.Name.Parts.LastOrDefault();
            switch (objectType.ToUpperInvariant())
            {
                case "PRIMARYKEYCONSTRAINT":
                    if (!Regex.IsMatch(name, $"^PK_{tableName}$", RegexOptions.IgnoreCase))
                    {
                        problems.Add(new SqlRuleProblem(MessageFormatter.FormatMessage($"Primary Key '{name}' does not follow the company naming standard. Please use the name PK_{tableName}.", RuleId), sqlObj, fragment));
                    }

                    break;
                case "INDEX":
                    var idx = fragment as CreateIndexStatement;

                    if (idx == null)
                    {
                        return problems;
                    }

                    var re = $"^IX_{tableName}_.*";
                    var naming = $"IX_{tableName}*";
                    if (idx.Unique)
                    {
                        re = $@"^UX_{tableName}_.*";
                        naming = $"UX_{tableName}*";
                    }

                    if (!Regex.IsMatch(name, re, RegexOptions.IgnoreCase))
                    {
                        problems.Add(new SqlRuleProblem(MessageFormatter.FormatMessage($"Index '{name}' does not follow the company naming standard. Please use a format that starts with {naming}.", RuleId), sqlObj, fragment));
                    }

                    break;
                case "FOREIGNKEYCONSTRAINT":
                    // var fk = fragment as createke;
                    var tableFk = ruleExecutionContext.SchemaModel.GetObject(ForeignKeyConstraint.TypeClass, sqlObj.Name, DacQueryScopes.All);
                    var foreignTableName = tableFk.GetReferencedRelationshipInstances(ForeignKeyConstraint.ForeignTable, DacQueryScopes.All)
                        .Select(x => x.ObjectName).ToList()
                        .First().Parts.LastOrDefault();

                    if (!Regex.IsMatch(name, $@"^FK_{tableName}_{foreignTableName}.*", RegexOptions.IgnoreCase))
                    {
                        problems.Add(new SqlRuleProblem(MessageFormatter.FormatMessage($"Foreign Key '{name}' does not follow the company naming standard. Please use a format that starts with FK_{tableName}_{foreignTableName}", RuleId), sqlObj, fragment));
                    }

                    break;
                case "CHECKCONSTRAINT":
                    if (!Regex.IsMatch(name, $@"^CK_{tableName}_.*", RegexOptions.IgnoreCase))
                    {
                        problems.Add(new SqlRuleProblem(MessageFormatter.FormatMessage($"Check Constraint '{name}' does not follow the company naming standard. Please use a format that starts with CK_{tableName}*.", RuleId), sqlObj, fragment));
                    }

                    break;
                case "DEFAULTCONSTRAINT":
                    var columnName = GetReferencedName(sqlObj, DefaultConstraint.TargetColumn, "Column");

                    // allow two formats for this one
                    if (!Regex.IsMatch(name, $@"^DF_{tableName}_{columnName}$", RegexOptions.IgnoreCase))
                    {
                        problems.Add(new SqlRuleProblem(MessageFormatter.FormatMessage($"Constraint '{name}' does not follow the company naming standard. Please use the name DF_{tableName}_{columnName}.", RuleId), sqlObj, fragment));
                    }

                    // ADD OTHER TYPES IF DESIRED IF YOU WANT THEM TO MATCH A SPECIFIC FORMAT
                    break;
            }

            return problems;
        }

        private static string GetReferencedName(TSqlObject sqlObj, ModelRelationshipClass relation = null, string typeToLookFor = "Table")
        {
            if (relation == null)
            {
                return sqlObj.GetReferenced().FirstOrDefault(o => Comparer.Equals(o.ObjectType.Name, typeToLookFor)).Name.Parts.LastOrDefault();
            }

            return sqlObj.GetReferenced(relation).FirstOrDefault(o => Comparer.Equals(o.ObjectType.Name, typeToLookFor)).Name.Parts.LastOrDefault();
        }
    }
}
