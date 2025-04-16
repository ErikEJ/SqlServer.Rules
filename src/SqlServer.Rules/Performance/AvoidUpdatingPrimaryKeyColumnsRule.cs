using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Performance
{
    /// <summary>Avoid updating columns that are part of the primary key.  (Halloween Protection)</summary>
    /// <FriendlyName>Update of Primary key</FriendlyName>
    /// <IsIgnorable>true</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// <list type="bullet">
    ///     <item> Halloween protection will be invoked causing the data being updated to be done in
    ///         a two step operation that spools to tempdb. </item>
    ///     <item> All the foreign keys that reference the updated key have to be also updated. If
    ///          the foreign keys are indexed, it will cause their indexes to be also updated, which
    ///          can be an expensive operation. Otherwise, if no index exists for foreign key
    ///          columns, a table lock will be applied. </item>
    ///     <item> When the table has Change Tracking enabled, the values of the primary key column
    ///          identify the rows that have been changed and this is the only information from the
    ///          tracked table that is recorded with the change information. If the synchronization
    ///          of the changed data is implemented based on the Change Tracking, it will fail
    ///          because of the modified primary key column values. </item>
    ///     <item> If this key is referenced in any external system the reference will be broken
    ///          upon update. </item>
    ///     <item> The primary keys are usually clustered. Updating the table's clustered index
    ///         will cause also update of the existing non-clustered indexes. </item>
    ///   </list>
    /// </remarks>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Performance,
        RuleScope = SqlRuleScope.Element)]
    public sealed class AvoidUpdatingPrimaryKeyColumnsRule : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRP0017";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Avoid updating columns that are part of the primary key.  (Halloween Protection)";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="AvoidUpdatingPrimaryKeyColumnsRule"/> class.
        /// </summary>
        public AvoidUpdatingPrimaryKeyColumnsRule()
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
            var model = ruleExecutionContext.SchemaModel;
            if (sqlObj == null || sqlObj.IsWhiteListed())
            {
                return problems;
            }

            var fragment = ruleExecutionContext.ScriptFragment?.GetFragment(ProgrammingSchemaTypes);

            if (fragment == null)
            {
                return problems;
            }

            var updateVisitor = new UpdateVisitor();
            fragment.Accept(updateVisitor);
            foreach (var update in updateVisitor.NotIgnoredStatements(RuleId))
            {
                if (!(update.UpdateSpecification.Target is NamedTableReference target) || target.GetName().Contains('#', System.StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // we have an aliased table we need to find out what the real table is so we can look up its columns
                if (update.UpdateSpecification.FromClause != null)
                {
                    var namedTableVisitor = new NamedTableReferenceVisitor { TypeFilter = ObjectTypeFilter.PermanentOnly };
                    update.UpdateSpecification.FromClause.Accept(namedTableVisitor);

                    var identifier = target.SchemaObject.Identifiers.LastOrDefault();

                    if (identifier == null)
                    {
                        continue;
                    }

                    var realTarget = namedTableVisitor.Statements
                        .FirstOrDefault(t => Comparer.Equals(t.Alias?.Value, identifier.Value));

                    if (realTarget == null)
                    {
                        continue;
                    }
                }

                var targetSqlObj = model.GetObject(Table.TypeClass, target.GetObjectIdentifier(), DacQueryScopes.All);

                var pk = targetSqlObj?.GetReferencing(PrimaryKeyConstraint.Host, DacQueryScopes.UserDefined).FirstOrDefault();
                if (pk == null)
                {
                    continue;
                }

                var primaryKeyColumns = pk.GetReferenced(PrimaryKeyConstraint.Columns, DacQueryScopes.All);

                var hasOffense = update.UpdateSpecification.SetClauses.OfType<AssignmentSetClause>().Any(setClause =>
                {
                    if (setClause.Column?.MultiPartIdentifier == null)
                    {
                        return false;
                    }

                    return primaryKeyColumns.Any(pkc => pkc.Name.CompareTo(setClause.Column?.MultiPartIdentifier) >= 5);
                });

                if (hasOffense)
                {
                    problems.Add(new SqlRuleProblem(MessageFormatter.FormatMessage(Message, RuleId), sqlObj, update));
                }
            }

            return problems;
        }
    }
}