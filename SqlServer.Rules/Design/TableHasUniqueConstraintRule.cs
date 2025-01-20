using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using SqlServer.Rules.Globals;
using Index = Microsoft.SqlServer.Dac.Model.Index;

namespace SqlServer.Rules.Design
{
    /// <summary>
    /// Tables should have a natural key
    /// </summary>
    /// <FriendlyName>Missing natural key</FriendlyName>
    /// <IsIgnorable>false</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class TableHasUniqueConstraintRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0001";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Table does not have a natural key.";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="TableHasUniqueConstraintRule"/> class.
        /// </summary>
        public TableHasUniqueConstraintRule()
            : base(ModelSchema.Table)
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

            var key = sqlObj.GetChildren(DacQueryScopes.All).FirstOrDefault(x => x.ObjectType == ModelSchema.PrimaryKeyConstraint);
            if (key == null)
            {
                return problems;
            }

            var keyColumns = key.GetReferenced().Where(x => x.ObjectType == Column.TypeClass).ToList();

            if (keyColumns.Count == 1)
            {
                var keyColumn = keyColumns.First();
                var isIdentity = keyColumn.GetProperty<bool?>(Column.IsIdentity);
                var keyColumnDefault = keyColumn.GetReferencing().FirstOrDefault(x => x.ObjectType == DefaultConstraint.TypeClass);
                var isSequence = keyColumnDefault?.GetReferenced().Any(x => x.ObjectType == Sequence.TypeClass);

                // if our primary key consists of a identity or sequence, check to see if we have a unique constraint. assume the unique constraint is a natural key if we find one. else, problem.
                if (isIdentity.GetValueOrDefault(false) || isSequence.GetValueOrDefault(false))
                {
                    var uc = sqlObj.GetChildren(DacQueryScopes.All).FirstOrDefault(x => x.ObjectType == ModelSchema.UniqueConstraint);
                    if (uc == null)
                    {
                        // no unique constraint. search for a unique index
                        var indexes = sqlObj.GetChildren(DacQueryScopes.All).Where(x => x.ObjectType == ModelSchema.Index);
                        if (!indexes.Any(ix => Convert.ToBoolean(ix.GetProperty(Index.Unique), CultureInfo.InvariantCulture)))
                        {
                            problems.Add(new SqlRuleProblem(Message, sqlObj));
                        }
                    }
                }
            }

            return problems;
        }
    }
}