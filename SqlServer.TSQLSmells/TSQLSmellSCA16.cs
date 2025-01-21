using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;

namespace TSQLSmellSCA
{
    [LocalizedExportCodeAnalysisRule(
        RuleId,
        RuleConstants.ResourceBaseName, // Name of the resource file to look up displayname and description in
        RuleConstants.TSQLSmell_RuleName16, // ID used to look up the display name inside the resources file
        null,
        Category = RuleConstants.CategorySmells,
        RuleScope = SqlRuleScope.Model)] // This rule targets the whole model
    public sealed class TSQLSmellSCA16 : SqlCodeAnalysisRule
    {
        public const string RuleId = "Smells.SML016";

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
            var Worker = new TSQLSmellWorker(ruleExecutionContext, RuleId);
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
            return Worker.Analyze();
        }
    }
}
