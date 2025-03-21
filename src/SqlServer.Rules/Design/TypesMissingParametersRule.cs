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
    public class TypesMissingParametersRule : BaseSqlCodeAnalysisRule
    {
        private readonly int expectParameterCount;
        private readonly string message;
        private readonly IList<SqlDataTypeOption> types;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypesMissingParametersRule"/> class.
        /// </summary>
        /// <param name="supportedElementTypes">The supported element types.</param>
        /// <param name="types">The types.</param>
        /// <param name="expectParameterCount">The expect parameter count.</param>
        /// <param name="message">The message.</param>
        public TypesMissingParametersRule(IList<ModelTypeClass> supportedElementTypes, IList<SqlDataTypeOption> types, int expectParameterCount, string message)
            : base(supportedElementTypes)
        {
            this.expectParameterCount = expectParameterCount;
            this.message = message;
            this.types = types;
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

            // ModelSchema.Procedure, ModelSchema.ScalarFunction, ModelSchema.TableValuedFunction, ModelSchema.Table
            var fragment = ruleExecutionContext.ScriptFragment?.GetFragment(
                typeof(CreateProcedureStatement),
                typeof(CreateFunctionStatement),
                typeof(CreateTableStatement));

            if (fragment == null)
            {
                return problems;
            }

            var variableVisitor = new VariablesVisitor();
            var tableDefinitionVisitor = new TableDefinitionVisitor();

            fragment.Accept(variableVisitor, tableDefinitionVisitor);

            var variables =
                from d in variableVisitor.DeclareVariables
                from v in d.Declarations
                let type = v.DataType as SqlDataTypeReference
                let typeOption = type?.SqlDataTypeOption
                where types.Contains(typeOption.GetValueOrDefault(SqlDataTypeOption.None))
                where type?.Parameters.Count != expectParameterCount
                select v;

            var parameters =
                from p in variableVisitor.ProcedureParameters
                let type = p.DataType as SqlDataTypeReference
                let typeOption = type?.SqlDataTypeOption
                where types.Contains(typeOption.GetValueOrDefault(SqlDataTypeOption.None))
                where type?.Parameters.Count != expectParameterCount
                select p;

            var columns =
                from s in tableDefinitionVisitor.Statements
                from c in s.ColumnDefinitions
                let type = c.DataType as SqlDataTypeReference
                let typeOption = type?.SqlDataTypeOption
                where types.Contains(typeOption.GetValueOrDefault(SqlDataTypeOption.None))
                where type?.Parameters.Count != expectParameterCount
                select c;

            problems.AddRange(variables.Select(p => new SqlRuleProblem(message, sqlObj, p)));
            problems.AddRange(parameters.Select(p => new SqlRuleProblem(message, sqlObj, p)));
            problems.AddRange(columns.Select(p => new SqlRuleProblem(message, sqlObj, p)));

            var castVisitor = new CastCallVisitor();
            var convertVisitor = new ConvertCallVisitor();
            fragment.Accept(castVisitor, convertVisitor);

            var castCalls = from c in castVisitor.Statements
                            let type = c.DataType as SqlDataTypeReference
                            let typeOption = type?.SqlDataTypeOption
                            where types.Contains(typeOption.GetValueOrDefault(SqlDataTypeOption.None))
                            where type?.Parameters.Count != expectParameterCount
                            select c;

            var convertCalls = from c in convertVisitor.Statements
                               let type = c.DataType as SqlDataTypeReference
                               let typeOption = type?.SqlDataTypeOption
                               where types.Contains(typeOption.GetValueOrDefault(SqlDataTypeOption.None))
                               where type?.Parameters.Count != expectParameterCount
                               select c;

            problems.AddRange(castCalls.Select(p => new SqlRuleProblem(message, sqlObj, p)));
            problems.AddRange(convertCalls.Select(p => new SqlRuleProblem(message, sqlObj, p)));

            return problems;
        }
    }
}