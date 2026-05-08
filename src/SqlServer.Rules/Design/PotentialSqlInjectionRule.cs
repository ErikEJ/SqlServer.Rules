using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Design
{
    /// <summary>
    /// Dynamic SQL built from procedure input can introduce SQL injection vulnerabilities.
    /// </summary>
    /// <FriendlyName>Potential SQL injection issue</FriendlyName>
    /// <IsIgnorable>false</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class PotentialSqlInjectionRule : BaseSqlCodeAnalysisRule
    {
        private const string SpExecuteSqlStmtParameter = "@stmt";

        public const string RuleId = Constants.RuleNameSpace + "SRD0096";
        public const string RuleDisplayName = "Potential SQL injection issue.";
        public const string Message = RuleDisplayName;

        public PotentialSqlInjectionRule()
            : base(ModelSchema.Procedure)
        {
        }

        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            var problems = new List<SqlRuleProblem>();
            var sqlObj = ruleExecutionContext.ModelElement;

            if (sqlObj == null || sqlObj.IsWhiteListed())
            {
                return problems;
            }

            var fragment = ruleExecutionContext.ScriptFragment?.GetFragment(typeof(CreateProcedureStatement));
            if (fragment == null)
            {
                return problems;
            }

            var taintedVariables = GetTaintedVariables(fragment);

            var executeVisitor = new ExecuteVisitor();
            fragment.Accept(executeVisitor);

            foreach (var execute in executeVisitor.NotIgnoredStatements(RuleId))
            {
                if (IsPotentialSqlInjection(execute, taintedVariables))
                {
                    problems.Add(new SqlRuleProblem(MessageFormatter.FormatMessage(Message, RuleId), sqlObj, execute));
                }
            }

            return problems;
        }

        private static HashSet<string> GetTaintedVariables(TSqlFragment fragment)
        {
            var taintedVariables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var variablesVisitor = new VariablesVisitor();
            fragment.Accept(variablesVisitor);

            foreach (var parameter in variablesVisitor.ProcedureParameters.Where(p => p.VariableName != null))
            {
                taintedVariables.Add(parameter.VariableName.Value);
            }

            var setVariableVisitor = new SetVariableStatementVisitor();
            fragment.Accept(setVariableVisitor);

            var declareVariableVisitor = new DeclareVariableElementVisitor();
            fragment.Accept(declareVariableVisitor);

            var changed = true;

            while (changed)
            {
                changed = false;

                foreach (var assignment in setVariableVisitor.NotIgnoredStatements(RuleId))
                {
                    if (assignment.Variable != null
                        && assignment.Expression != null
                        && ExpressionReferencesTaintedVariable(assignment.Expression, taintedVariables)
                        && taintedVariables.Add(assignment.Variable.Name))
                    {
                        changed = true;
                    }
                }

                foreach (var assignment in variablesVisitor.SelectSetVariables)
                {
                    if (assignment.Variable != null
                        && assignment.Expression != null
                        && ExpressionReferencesTaintedVariable(assignment.Expression, taintedVariables)
                        && taintedVariables.Add(assignment.Variable.Name))
                    {
                        changed = true;
                    }
                }

                foreach (var declaration in declareVariableVisitor.NotIgnoredStatements(RuleId))
                {
                    if (declaration.VariableName != null
                        && declaration.Value != null
                        && ExpressionReferencesTaintedVariable(declaration.Value, taintedVariables)
                        && taintedVariables.Add(declaration.VariableName.Value))
                    {
                        changed = true;
                    }
                }
            }

            return taintedVariables;
        }

        private static bool IsPotentialSqlInjection(ExecuteStatement execute, HashSet<string> taintedVariables)
        {
            if (execute.ExecuteSpecification?.ExecutableEntity is ExecutableStringList stringList)
            {
                return stringList.Strings
                    .OfType<VariableReference>()
                    .Any(variable => taintedVariables.Contains(variable.Name));
            }

            if (execute.ExecuteSpecification?.ExecutableEntity is not ExecutableProcedureReference procReference)
            {
                return false;
            }

            var procName = procReference.ProcedureReference?.ProcedureReference?.Name?.BaseIdentifier?.Value;
            if (!string.Equals(procName, "sp_executesql", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            foreach (var parameter in procReference.Parameters)
            {
                if (parameter.Variable?.Name.Equals(SpExecuteSqlStmtParameter, StringComparison.OrdinalIgnoreCase) == true
                    && parameter.ParameterValue != null
                    && ExpressionReferencesTaintedVariable(parameter.ParameterValue, taintedVariables))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ExpressionReferencesTaintedVariable(ScalarExpression expression, HashSet<string> taintedVariables)
        {
            var variableVisitor = new VariableReferenceVisitor();
            expression.Accept(variableVisitor);
            return variableVisitor.Statements.Any(v => taintedVariables.Contains(v.Name));
        }
    }
}
