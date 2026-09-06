using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Design
{
    /// <summary>
    /// UPDATE statement without row limiting conditions.
    /// </summary>
    /// <FriendlyName>Unbounded UPDATE</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// The rule looks for <c>UPDATE</c> statements not having a <c>WHERE</c> clause. Consider
    /// reviewing your code to avoid unintentionally updating all the records in the table.
    /// </remarks>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class AvoidUpdateWithoutWhereRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0018";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "UPDATE statement without row limiting conditions.";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="AvoidUpdateWithoutWhereRule"/> class.
        /// </summary>
        public AvoidUpdateWithoutWhereRule()
            : base(ProgrammingSchemas)
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

            var fragment = ruleExecutionContext.ScriptFragment?.GetFragment(ProgrammingSchemaTypes);

            if (fragment == null)
            {
                return problems;
            }

            var visitor = new UpdateVisitor();

            fragment.Accept(visitor);

            foreach (var stmt in visitor.NotIgnoredStatements(RuleId))
            {
                if (stmt.UpdateSpecification.WhereClause != null
                    || !(stmt.UpdateSpecification.Target is NamedTableReference reference))
                {
                    continue;
                }

                var targetAliasOrName = reference.SchemaObject.Identifiers.Last().Value;
                var tableName = targetAliasOrName;

                if (stmt.UpdateSpecification.FromClause != null)
                {
                    var tableVisitor = new TableReferenceVisitor();
                    stmt.UpdateSpecification.FromClause.Accept(tableVisitor);

                    var table = tableVisitor.Statements.OfType<NamedTableReference>()
                        .FirstOrDefault(t => Comparer.Equals(t.Alias?.Value, targetAliasOrName));
                    if (table != null)
                    {
                        tableName = table.SchemaObject.Identifiers.Last().Value;
                    }

                    var joinVisitor = new JoinVisitor();
                    stmt.UpdateSpecification.FromClause.Accept(joinVisitor);

                    if (joinVisitor.QualifiedJoins.Any(join =>
                        (ContainsTargetReference(join.FirstTableReference, targetAliasOrName, tableName)
                        || ContainsTargetReference(join.SecondTableReference, targetAliasOrName, tableName))
                        && JoinSearchConditionReferencesTarget(join, targetAliasOrName, tableName)))
                    {
                        continue;
                    }
                }

                if (!(tableName.StartsWith('#') || tableName.StartsWith('@')))
                {
                    problems.Add(new SqlRuleProblem(MessageFormatter.FormatMessage(Message, RuleId), sqlObj, stmt));
                }
            }

            return problems;
        }

        private static bool JoinSearchConditionReferencesTarget(QualifiedJoin join, string targetAliasOrName, string targetTableName)
        {
            if (join.SearchCondition == null)
            {
                return false;
            }

            var columnVisitor = new ColumnReferenceExpressionVisitor();
            join.SearchCondition.Accept(columnVisitor);

            var qualifiers = columnVisitor.Statements
                .Select(GetTableOrAliasQualifier)
                .Where(q => !string.IsNullOrWhiteSpace(q))
                .ToList();

            var referencesTarget = qualifiers.Any(qualifier =>
            {
                return Comparer.Equals(qualifier, targetAliasOrName)
                    || Comparer.Equals(qualifier, targetTableName);
            });

            if (!referencesTarget)
            {
                return false;
            }

            return qualifiers.Any(qualifier =>
                !Comparer.Equals(qualifier, targetAliasOrName)
                && !Comparer.Equals(qualifier, targetTableName));
        }

        private static string? GetTableOrAliasQualifier(ColumnReferenceExpression column)
        {
            var identifiers = column.MultiPartIdentifier?.Identifiers;
            if (identifiers == null || identifiers.Count < 2)
            {
                return null;
            }

            return identifiers[identifiers.Count - 2].Value;
        }

        private static bool ContainsTargetReference(TableReference tableReference, string targetAliasOrName, string targetTableName)
        {
            return tableReference switch
            {
                NamedTableReference named => Comparer.Equals(named.Alias?.Value, targetAliasOrName)
                    || Comparer.Equals(named.SchemaObject.Identifiers.Last().Value, targetTableName),
                QualifiedJoin qualified => ContainsTargetReference(qualified.FirstTableReference, targetAliasOrName, targetTableName)
                    || ContainsTargetReference(qualified.SecondTableReference, targetAliasOrName, targetTableName),
                UnqualifiedJoin unqualified => ContainsTargetReference(unqualified.FirstTableReference, targetAliasOrName, targetTableName)
                    || ContainsTargetReference(unqualified.SecondTableReference, targetAliasOrName, targetTableName),
                JoinParenthesisTableReference parenthesized => ContainsTargetReference(parenthesized.Join, targetAliasOrName, targetTableName),
                _ => false,
            };
        }
    }
}