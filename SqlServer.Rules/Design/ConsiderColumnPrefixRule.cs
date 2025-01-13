using System;
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
    ///
    /// </summary>
    /// <FriendlyName></FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class ConsiderColumnPrefixRule : BaseSqlCodeAnalysisRule

    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0028";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Consider prefixing column names with table name or table alias.";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = "Consider prefixing column names with table name or table alias. Preferably an alias.";

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsiderColumnPrefixRule"/> class.
        /// </summary>
        public ConsiderColumnPrefixRule() : base(ProgrammingAndViewSchemas)
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

            var fragment = ruleExecutionContext.ScriptFragment.GetFragment(ProgrammingAndViewSchemaTypes);
            var selectStatementVisitor = new SelectStatementVisitor();
            fragment.Accept(selectStatementVisitor);

            if (selectStatementVisitor.Count == 0) { return problems; }

            foreach (var select in selectStatementVisitor.Statements)
            {
                if (select.QueryExpression is QuerySpecification query)
                {
                    var fromClause = query.FromClause;
                    if (fromClause == null) { continue; }

                    // check to ensure we have more than one table
                    var namedTableVisitor = new NamedTableReferenceVisitor();
                    fromClause.Accept(namedTableVisitor);
                    if (namedTableVisitor.Count <= 1) { continue; }

                    var columnReferences = new ColumnReferenceExpressionVisitor();
                    query.Accept(columnReferences);

                    var offenders = columnReferences.NotIgnoredStatements(RuleId)
                        .Where(c => CheckName(c))
                        .Select(n => n.MultiPartIdentifier.Identifiers[0]);

                    if (offenders.Any())
                    {
                        problems.Add(new SqlRuleProblem(Message, sqlObj, select));
                    }
                }
            }

            return problems;
        }

        private static bool CheckName(ColumnReferenceExpression col)
        {
            var names = col.MultiPartIdentifier?.Identifiers;
            if (names == null) { return false; }
            return names.Count == 1 && !Constants.DateParts.Contains(names.First().Value, StringComparer.OrdinalIgnoreCase);
        }
    }
}
