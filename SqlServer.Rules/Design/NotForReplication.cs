using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Design
{
    /// <summary>
    /// Avoid 'NOT FOR REPLICATION' unless this is the desired behavior and replication is in use.
    /// </summary>
    /// <FriendlyName>Avoid NOT FOR REPLICATION</FriendlyName>
    /// <IsIgnorable>false</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <seealso cref="SqlServer.Rules.BaseSqlCodeAnalysisRule" />
    [ExportCodeAnalysisRule(RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class NotForReplication : BaseSqlCodeAnalysisRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0065";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = Message;

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = "Avoid 'NOT FOR REPLICATION' unless this is the desired behavior and replication is in use.";

        /// <summary>
        /// Initializes a new instance of the <see cref="NotForReplication"/> class.
        /// </summary>
        public NotForReplication() : base(
            ModelSchema.ForeignKeyConstraint,
            ModelSchema.CheckConstraint,
            ModelSchema.DmlTrigger,
            ModelSchema.Table)
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

            var obj = ruleExecutionContext.SchemaModel.GetObject(sqlObj.ObjectType, sqlObj.Name, DacQueryScopes.All);

            var notForReplication = false;

            if (sqlObj.ObjectType == ForeignKeyConstraint.TypeClass)
            {
                notForReplication = Convert.ToBoolean(sqlObj.GetProperty(ForeignKeyConstraint.NotForReplication), CultureInfo.InvariantCulture);
            }
            else if (sqlObj.ObjectType == CheckConstraint.TypeClass)
            {
                notForReplication = Convert.ToBoolean(sqlObj.GetProperty(CheckConstraint.NotForReplication), CultureInfo.InvariantCulture);
            }
            else if (sqlObj.ObjectType == DmlTrigger.TypeClass)
            {
                notForReplication = Convert.ToBoolean(sqlObj.GetProperty(DmlTrigger.NotForReplication), CultureInfo.InvariantCulture);
            }
            else if (sqlObj.ObjectType == Table.TypeClass)
            {
                var createTable = ruleExecutionContext.ScriptFragment as CreateTableStatement;
                var identityColumn = createTable.Definition.ColumnDefinitions.FirstOrDefault(cd => cd.IdentityOptions != null);
                if (identityColumn != null)
                {
                    notForReplication = identityColumn.IdentityOptions.IsIdentityNotForReplication;
                }
            }

            if (notForReplication)
            {
                problems.Add(new SqlRuleProblem(Message, sqlObj));
            }

            return problems;
        }
    }
}