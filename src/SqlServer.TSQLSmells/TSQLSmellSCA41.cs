﻿using System.Collections.Generic;
using Microsoft.SqlServer.Dac.CodeAnalysis;

namespace TSQLSmellSCA
{
    [LocalizedExportCodeAnalysisRule(
        RuleId,
        RuleConstants.ResourceBaseName, // Name of the resource file to look up displayname and description in
        RuleConstants.TSQLSmellRuleName41, // ID used to look up the display name inside the resources file
        null,
        Category = RuleConstants.CategorySmells,
        RuleScope = SqlRuleScope.Model)] // This rule targets the whole model
    public sealed class TSQLSmellSCA41 : SqlCodeAnalysisRule
    {
        public const string RuleId = "Smells.SML041";

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            var worker = new TSQLSmellWorker(ruleExecutionContext, RuleId);
            return worker.Analyze();
        }
    }
}
