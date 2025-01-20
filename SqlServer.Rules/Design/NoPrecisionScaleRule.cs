using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Rules.Globals;

namespace SqlServer.Rules.Design
{
    /// <summary>
    /// Do not use DECIMAL or NUMERIC data types without specifying precision and scale.
    /// </summary>
    /// <FriendlyName>Unspecified precision or scale </FriendlyName>
    /// <IsIgnorable>false</IsIgnorable>
    /// <ExampleMd></ExampleMd>
    /// <remarks>
    /// The rule checks the T-SQL code for use <c>DECIMAL</c> or <c>NUMERIC</c> data types without
    /// specifying length. Avoid defining columns, variables, and parameters using
    /// <c>DECIMAL</c> or <c>NUMERIC</c> data types without specifying precision, and scale. If no
    /// precision and scale are provided, SQL Server will use its default size <c>NUMERIC(18, 0)</c>.
    /// </remarks>
    /// <seealso cref="SqlServer.Rules.Design.TypesMissingParametersRule" />
    [ExportCodeAnalysisRule(
        RuleId,
        RuleDisplayName,
        Description = RuleDisplayName,
        Category = Constants.Design,
        RuleScope = SqlRuleScope.Element)]
    public sealed class NoPrecisionScaleRule : TypesMissingParametersRule
    {
        /// <summary>
        /// The rule identifier
        /// </summary>
        public const string RuleId = Constants.RuleNameSpace + "SRD0027";

        /// <summary>
        /// The rule display name
        /// </summary>
        public const string RuleDisplayName = "Do not use DECIMAL or NUMERIC data types without specifying precision and scale.";

        /// <summary>
        /// The message
        /// </summary>
        public const string Message = RuleDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="NoPrecisionScaleRule"/> class.
        /// </summary>
        public NoPrecisionScaleRule()
            : base(
            new[]
                {
                    ModelSchema.Procedure, ModelSchema.ScalarFunction, ModelSchema.TableValuedFunction, ModelSchema.Table,
                }, new[] { SqlDataTypeOption.Decimal, SqlDataTypeOption.Numeric }, 2, Message)
        {
        }
    }
}