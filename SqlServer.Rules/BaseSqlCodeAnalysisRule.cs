using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;
using SqlServer.Dac.Visitors;

namespace SqlServer.Rules
{
    /// <summary>
    /// The base code analysis rule for all other rules.
    /// </summary>
    /// <seealso cref="Microsoft.SqlServer.Dac.CodeAnalysis.SqlCodeAnalysisRule" />
    public abstract class BaseSqlCodeAnalysisRule : SqlCodeAnalysisRule
    {
        /// <summary>
        /// The programming schemas
        /// </summary>
        protected static readonly IList<ModelTypeClass> ProgrammingSchemas = new[] { ModelSchema.Procedure, ModelSchema.ScalarFunction, ModelSchema.TableValuedFunction };

        /// <summary>
        /// The programming and view schemas
        /// </summary>
        protected static readonly IList<ModelTypeClass> ProgrammingAndViewSchemas = new[] { ModelSchema.Procedure, ModelSchema.ScalarFunction, ModelSchema.TableValuedFunction, ModelSchema.View };

        /// <summary>
        /// The programming schema types
        /// </summary>
        protected static readonly Type[] ProgrammingSchemaTypes = new Type[] { typeof(CreateProcedureStatement), typeof(CreateFunctionStatement) };

        /// <summary>
        /// The programming and view schema types
        /// </summary>
        protected static readonly Type[] ProgrammingAndViewSchemaTypes = new Type[] { typeof(CreateProcedureStatement), typeof(CreateFunctionStatement), typeof(CreateViewStatement) };

        /// <summary>
        /// The comparer
        /// </summary>
#pragma warning disable CA2211 // Non-constant fields should not be visible
#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable SA1401 // Fields should be private
        public static StringComparer Comparer = StringComparer.InvariantCultureIgnoreCase;
#pragma warning restore SA1401 // Fields should be private
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore CA2211 // Non-constant fields should not be visible
        /// <summary>
        /// Gets the problems.
        /// </summary>
        /// <value>
        /// The problems.
        /// </value>
#pragma warning disable CA1002 // Do not expose generic lists
        protected List<SqlRuleProblem> Problems { get; } = new List<SqlRuleProblem>();
#pragma warning restore CA1002 // Do not expose generic lists

        // really not proud of this... could not figure out another way. has to be maintained with each new SQL Server version.
        private static readonly Dictionary<string, string> _functions = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
        {
            /*Date and Time Data Types and Functions (Transact-SQL)*/
            { "CURRENT_DATE", "date" },
            { "CURRENT_TIMESTAMP", "datetime" },
            { "CURRENT_TIMEZONE", "varchar" },
            { "CURRENT_TIMEZONE_ID", "varchar" },
            { "DATEDIFF", "int" },
            { "DATEDIFF_BIG", "bigint" },
            { "DATEFROMPARTS", "date" },
            { "DATENAME", "nvarchar" },
            { "DATEPART", "int" },
            { "DATETIME2FROMPARTS", "datetime2" },
            { "DATETIMEFROMPARTS", "datetime" },
            { "DATETIMEOFFSETFROMPARTS", "datetimeoffset" },
            { "DAY", "int" },
            { "EOMONTH", "date" },
            { "GETDATE", "datetime" },
            { "GETUTCDATE", "datetime" },
            { "ISDATE", "int" },
            { "MONTH", "int" },
            { "SMALLDATETIMEFROMPARTS", "smalldatetime" },
            { "SWITCHOFFSET", "datetimeoffset" },
            { "SYSDATETIME", "datetime2" },
            { "SYSDATETIMEOFFSET", "datetimeoffset" },
            { "SYSUTCDATETIME", "datetime2" },
            { "TIMEFROMPARTS", "time" },
            { "TODATETIMEOFFSET", "datetimeoffset" },
            { "YEAR", "int" },
            /* Mathematical Functions (Transact-SQL)*/
            { "ACOS", "float" },
            { "ASIN", "float" },
            { "ATAN", "float" },
            { "ATN2", "float" },
            { "COS", "float" },
            { "COT", "float" },
            { "EXP", "float" },
            { "LOG", "float" },
            { "LOG10", "float" },
            { "PI", "float" },
            { "POWER", "float" },
            { "RAND", "float" },

            // { "ROUND", "" }, completely unable to figure out how to map these. leaving commented here to mark that in case someone else figures it out
            // { "SIGN", "" },
            { "SIN", "float" },
            { "SQRT", "float" },
            { "SQUARE", "float" },
            { "TAN", "float" },
            /*String Functions (Transact-SQL)*/
            { "ASCII", "int" },
            { "BASE64_DECODE", "varbinary" },
            { "BASE64_ENCODE", "varchar" },
            { "CHAR", "char" },
            { "DIFFERENCE", "int" },
            { "FORMAT", "nvarchar" },
            { "QUOTENAME", "nvarchar" },
            { "SOUNDEX", "varchar" },
            { "SPACE", "varchar" },
            { "STR", "varchar" },
            { "STRING_ESCAPE", "nvarchar" },
            { "UNICODE", "int" },
            /* System Functions (Transact-SQL)*/
            { "HOST_ID", "char" },
            { "HOST_NAME", "nvarchar" },
            { "ISNUMERIC", "int" },
            { "NEWID", "uniqueidentifier" },
            { "NEWSEQUENTIALID", "uniqueidentifier" },
            { "ROWCOUNT_BIG", "bigint" },
            { "SESSION_CONTEXT", "sql_variant" },
            { "SESSION_ID", "nvarchar" },
            { "XACT_STATE", "smallint" },
            /*Metadata Functions (Transact-SQL)*/
            { "APP_NAME", "nvarchar" },
            { "APPLOCK_MODE", "nvarchar" },
            { "APPLOCK_TEST", "smallint" },
            { "ASSEMBLYPROPERTY", "sql_variant" },
            { "COL_LENGTH", "smallint" },
            { "COL_NAME", "nvarchar" },
            { "COLUMNPROPERTY", "int" },
            { "DATABASEPROPERTYEX", "sql_variant" },
            { "DB_ID", "int" },
            { "DB_NAME", "nvarchar" },
            { "FILE_ID", "smallint" },
            { "FILE_IDEX", "int" },
            { "FILE_NAME", "nvarchar" },
            { "FILEGROUP_ID", "int" },
            { "FILEGROUP_NAME", "nvarchar" },
            { "FILEGROUPPROPERTY", "int" },
            { "FILEPROPERTY", "int" },
            { "FULLTEXTCATALOGPROPERTY", "int" },
            { "FULLTEXTSERVICEPROPERTY", "int" },
            { "INDEX_COL", "nvarchar" },
            { "INDEXKEY_PROPERTY", "int" },
            { "INDEXPROPERTY", "int" },
            { "OBJECT_DEFINITION", "nvarchar" },
            { "OBJECT_ID", "int" },
            { "OBJECT_NAME", "sysname" },
            { "OBJECT_SCHEMA_NAME", "sysname" },
            { "OBJECTPROPERTY", "int" },
            { "OBJECTPROPERTYEX", "sql_variant" },
            { "ORIGINAL_DB_NAME", "nvarchar" },
            { "PARSENAME", "nchar" },
            { "SCHEMA_ID", "int" },
            { "SCHEMA_NAME", "sysname" },
            { "SCOPE_IDENTITY", "numeric" },
            { "SERVERPROPERTY", "sql_variant" },
            { "STATS_DATE", "datetime" },
            { "TYPE_ID", "int" },
            { "TYPE_NAME", "sysname" },
            { "TYPEPROPERTY", "int" },
            /*Security Functions (Transact-SQL)*/
            { "CERTENCODED", "varbinary" },
            { "CERTPRIVATEKEY", "varbinary" },
            { "CURRENT_USER", "sysname" },
            { "DATABASE_PRINCIPAL_ID", "int" },
            { "HAS_PERMS_BY_NAME", "int" },
            { "IS_MEMBER", "int" },
            { "IS_ROLEMEMBER", "int" },
            { "IS_SRVROLEMEMBER", "int" },
            { "ORIGINAL_LOGIN", "sysname" },
            { "PERMISSIONS", "int" },
            { "PWDCOMPARE", "int" },
            { "PWDENCRYPT", "varbinary" },
            { "SESSION_USER", "nvarchar" },
            { "SUSER_ID", "int" },
            { "SUSER_SID", "varbinary" },
            { "SUSER_SNAME", "nvarchar" },
            { "SYSTEM_USER", "nchar" },
            { "SUSER_NAME", "nvarchar" },
            { "USER_ID", "int" },
            { "USER_NAME", "nvarchar" },
        };

        /// <summary>
        /// Gets the statement list.
        /// </summary>
        /// <param name="fragment">The fragment.</param>
        /// <returns></returns>
        public static StatementList GetStatementList(TSqlFragment fragment)
        {
            var fragmentTypeName = fragment.GetType().Name;
            var statementList = new StatementList();

            switch (fragmentTypeName.ToUpperInvariant())
            {
                case "CREATEPROCEDURESTATEMENT":
                    return (fragment as CreateProcedureStatement)?.StatementList;

                case "CREATEVIEWSTATEMENT":
                    statementList.Statements.Add((fragment as CreateViewStatement)?.SelectStatement);
                    return statementList;

                case "CREATEFUNCTIONSTATEMENT":
                    var func = (fragment as CreateFunctionStatement);
                    if (func == null) { return null; }

                    // this is an ITVF, and does not have a statement list, it has one statement in the return block...
                    if (func.StatementList == null && func.ReturnType is SelectFunctionReturnType returnType)
                    {
                        statementList.Statements.Add(returnType.SelectStatement);
                        return statementList;
                    }

                    return func.StatementList;

                case "CREATETRIGGERSTATEMENT":
                    return (fragment as CreateTriggerStatement)?.StatementList;

                default:
                    // throw new ApplicationException("Unable to determine statement list for fragment type: " + fragmentTypeName);
                    return null;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseSqlCodeAnalysisRule"/> class.
        /// </summary>
        /// <param name="supportedElementTypes">The supported element types.</param>
        protected BaseSqlCodeAnalysisRule(IList<ModelTypeClass> supportedElementTypes)
        {
            SupportedElementTypes = supportedElementTypes;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseSqlCodeAnalysisRule"/> class.
        /// </summary>
        /// <param name="supportedElementTypes">The supported element types.</param>
        protected BaseSqlCodeAnalysisRule(params ModelTypeClass[] supportedElementTypes)
        {
            SupportedElementTypes = supportedElementTypes;
        }

        /// <summary>
        /// Gets the data type of the value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        protected static string GetDataType(IntegerLiteral value)
        {
            return value.LiteralType.ToString();
        }

        /// <summary>
        /// Gets the data type of the value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        protected static string GetDataType(NumericLiteral value)
        {
            return value.LiteralType.ToString();
        }

        /// <summary>
        /// Gets the data type of the value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        protected static string GetDataType(StringLiteral value)
        {
            if (value.IsNational) { return "nvarchar"; }
            return "varchar";
        }

        /// <summary>
        /// Gets the data type of the value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        protected static string GetDataType(ScalarExpression value)
        {
            if (value is IntegerLiteral exprInt)
            {
                return GetDataType(exprInt);
            }

            if (value is NumericLiteral exprNum)
            {
                return GetDataType(exprNum);
            }

            if (value is FunctionCall exprFunc)
            {
                if (_functions.TryGetValue(exprFunc.FunctionName.Value, out var type))
                {
                    return type;
                }
            }
            else if (value is BinaryExpression exprBin)
            {
                return GetDataType(exprBin.FirstExpression);
            }
            else if (value is StringLiteral exprStr)
            {
                return GetDataType(exprStr);
            }

            return null;
        }

        /// <summary>
        /// Gets the data type of the value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="variables">The variables.</param>
        /// <returns></returns>
        protected static string GetDataType(ScalarExpression value, IList<DataTypeView> variables)
        {
            if (!(value is VariableReference varRef)) { return GetDataType(value); }

            var var1 = variables.FirstOrDefault(v => Comparer.Equals(v.Name, varRef.Name));
            if (var1 != null) { return var1.DataType; }

            return string.Empty;
        }

        /// <summary>
        /// Gets the data type of the value.
        /// </summary>
        /// <param name="sqlObj">The SQL object.</param>
        /// <param name="query">The query.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="variables">The variables.</param>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        protected static string GetDataType(TSqlObject sqlObj,
            QuerySpecification query,
            ScalarExpression expression,
            IList<DataTypeView> variables, TSqlModel model = null)
        {
            if (expression == null) { return null; }

            if (expression is ColumnReferenceExpression expression1)
            {
                return GetColumnDataType(sqlObj, query, expression1, model, variables);
            }

            if (expression is StringLiteral stringLiteral)
            {
                if (stringLiteral.IsNational)
                {
                    return "nvarchar";
                }

                return "varchar";
            }

            if (expression is NumericLiteral exprNum)
            {
                return exprNum.LiteralType.ToString();
            }

            if (expression is IntegerLiteral exprInt)
            {
                var val = long.Parse(exprInt.Value, CultureInfo.InvariantCulture);

                if (val >= 0 && val <= 255) // to bit or not to bit? NFC.
                {
                    return "tinyint";
                }

                if (val >= -32768 && val <= 32768)
                {
                    return "smallint";
                }

                if (val >= -2147483648 && val <= 2147483648)
                {
                    return "int";
                }

                if (val >= -9223372036854775808 && val <= 9223372036854775807)
                {
                    return "bigint";
                }

                // technically this may not be accurate. as sql sever will interpret literal ints as different types
                // depending upon how large they are. smallint, tinyint, etc... Unless I mimic their same value behavior.
                return "int";
            }

            if (expression is CastCall exprCast)
            {
                return exprCast.DataType.Name.Identifiers.First().Value;
            }

            if (expression is ConvertCall exprConvert)
            {
                return exprConvert.DataType.Name.Identifiers.First().Value;
            }

            if (expression is VariableReference exprVar)
            {
                var variable = variables.FirstOrDefault(v => Comparer.Equals(v.Name, exprVar.Name));
                if (variable != null)
                {
                    return variable.DataType;
                }
            }
            else if (expression is FunctionCall exprFunc)
            {
                // TIM C: sigh, this does not work for all functions. the api does not allow for me to look up built in functions. nor does it allow me to get the
                // data types of parameters, so I am not able to type ALL functions like DATEADD, the parameter could be a column, string literal, variable, function etc...
                if (_functions.TryGetValue(exprFunc.FunctionName.Value, out var type))
                {
                    return type;
                }
            }
            else if (expression is BinaryExpression exprBin)
            {
                var datatype1 = GetDataType(sqlObj, query, exprBin.FirstExpression, variables, model);
                if (datatype1 != null) { return datatype1; }
                return GetDataType(sqlObj, query, exprBin.SecondExpression, variables, model);
            }
            else if (expression is ScalarSubquery exprScalar)
            {
                var scalarQuery = exprScalar.QueryExpression as QuerySpecification;
                var selectElement = scalarQuery.SelectElements.First();

                return GetDataType(sqlObj, scalarQuery, ((SelectScalarExpression)selectElement).Expression, variables, model);
            }
            else if (expression is IIfCall exprIf)
            {
                return GetDataType(sqlObj, query, exprIf.ThenExpression, variables, model);
            }
            else
            {
                Debug.WriteLine("Unknown expression");
            }

            return null;
        }

        // protected string GetColumnDataType(ColumnReferenceExpression value, Dictionary<NamedTableView, IDictionary<string, DataTypeView>> columnDataTypes)
        // {
        //    var columnName = value.MultiPartIdentifier.Identifiers.GetName().ToLower();
        //    var types = columnDataTypes.Where(t => t.Key.Name.ToLower().Contains(columnName));
        //    //so.... technically this could resolve to multiple columns, but I have no clue which one to pick as the column does not have any reference to the parent query.
        //    var typ = types.FirstOrDefault();

        // if (typ.Key != null)
        //    {
        //        //return typ.Value..DataType.;
        //    }

        // return null;
        // }

        /// <summary>
        /// Gets the data type of the column.
        /// </summary>
        /// <param name="sqlObj">The SQL object.</param>
        /// <param name="query">The query.</param>
        /// <param name="column">The column.</param>
        /// <param name="model">The model.</param>
        /// <param name="variables">The variables.</param>
        /// <returns></returns>
        protected static string GetColumnDataType(TSqlObject sqlObj, QuerySpecification query, ColumnReferenceExpression column, TSqlModel model, IList<DataTypeView> variables)
        {
            TSqlObject referencedColumn = null;

            var columnName = column.MultiPartIdentifier.Identifiers.Last().Value;
            var columns = sqlObj.GetReferenced(DacQueryScopes.All).Where(x =>
                x.ObjectType == Column.TypeClass &&
                x.Name.GetName().Contains($"[{columnName}]", StringComparison.OrdinalIgnoreCase))
                .Distinct().ToList();

            if (columns.Count == 0)
            {
                // we have an aliased column, probably from a cte, temp table, or sub-select. we need to try to find it
                var visitor = new SelectScalarExpressionVisitor();
                sqlObj.GetFragment().Accept(visitor); // sqlObj.GetFragment()

                // try to find a select column where the alias matches the column name we are searching for
                var selectColumns = visitor.Statements.Where(x => Comparer.Equals(x.ColumnName?.Value, columnName)).ToList();

                // if we find more than one match, we have no way to determine which is the correct one.
                if (selectColumns.Count == 1)
                {
                    return GetDataType(sqlObj, query, selectColumns.First().Expression, variables);
                }

                return null;
            }

            if (columns.Count > 1)
            {
                var tablesVisitor = new TableReferenceWithAliasVisitor();

                if (column.MultiPartIdentifier.Identifiers.Count > 1)
                {
                    sqlObj.GetFragment().Accept(tablesVisitor);

                    var columnTableAlias = column.MultiPartIdentifier.Identifiers.First().Value;
                    var tbls = tablesVisitor.Statements
                        .Where(x => Comparer.Equals(x.Alias?.Value, columnTableAlias) || Comparer.Equals(x.GetName(), $"[{columnTableAlias}]"))
                        .ToList();

                    // if we find more than one table with the same alias, we have no idea which one it could be.
                    if (tbls.Count == 1)
                    {
                        referencedColumn = GetReferencedColumn(tbls.FirstOrDefault(), columns, columnName);
                    }
                    else
                    {
                        foreach (var tbl in tbls)
                        {
                            referencedColumn = GetReferencedColumn(tbl, columns, columnName);
                            if (referencedColumn != null) { break; }
                        }
                    }
                }
                else
                {
                    query.Accept(tablesVisitor);
                    if (tablesVisitor.Count == 1)
                    {
                        referencedColumn = GetReferencedColumn(tablesVisitor.Statements.FirstOrDefault(), columns, columnName);
                    }
                    else
                    {
                        foreach (var tbl in tablesVisitor.Statements)
                        {
                            referencedColumn = GetReferencedColumn(tbl, columns, columnName);
                            if (referencedColumn != null) { break; }
                        }
                    }
                }
            }
            else
            {
                referencedColumn = columns.FirstOrDefault();
            }

            if (referencedColumn != null)
            {
                TSqlObject dataType = null;

                // sometimes for some reason, I have to call getreferenced multiple times to get to the datatype. nfc why....
                while (dataType == null && referencedColumn != null)
                {
                    var colReferenced = referencedColumn.GetReferenced(DacQueryScopes.All).ToList();

                    dataType = colReferenced.FirstOrDefault(x => Comparer.Equals(x.ObjectType.Name, "DataType"));
                    if (dataType == null)
                    {
                        // try the next? referenced column.
                        referencedColumn = colReferenced.FirstOrDefault(x => x.ObjectType == Column.TypeClass);
                    }
                    else
                    {
                        break;
                    }
                }

                if (dataType != null)
                {
                    return dataType.Name.Parts.First();
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the referenced column.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="columns">The columns.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <returns></returns>
        private static TSqlObject GetReferencedColumn(TableReference table, List<TSqlObject> columns, string columnName)
        {
            TSqlObject referencedColumn = null;

            if (table == null) { return referencedColumn; }

            if (table is NamedTableReference reference)
            {
                Func<string, string, string, bool> compareNames = (string t1, string t2, string c) =>
                    (t1.Contains($"{t2}.[{c}]", StringComparison.OrdinalIgnoreCase) 
                        || t1.Contains($"[{c}]", StringComparison.OrdinalIgnoreCase) 
                    && !t1.Contains('#', StringComparison.OrdinalIgnoreCase));
                var tableName = reference.GetName();
                referencedColumn = columns.FirstOrDefault(c => compareNames(c.Name.GetName(), tableName, columnName));
            }
            else if (table is VariableTableReference reference1)
            {
                var tableName = reference1.Variable.Name;
                referencedColumn = columns.FirstOrDefault(c => c.Name.GetName().Contains($"[{tableName}].[{columnName}]", StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                referencedColumn = columns.FirstOrDefault(c => c.Name.GetName().Contains($"[{columnName}]", StringComparison.OrdinalIgnoreCase));
                Debug.WriteLine($"Unknown table type:{table.GetType().Name}");
            }

            return referencedColumn;
        }

        /// <summary>
        /// Gets the table from column.
        /// </summary>
        /// <param name="sqlObj">The SQL object.</param>
        /// <param name="query">The query.</param>
        /// <param name="column">The column.</param>
        /// <returns></returns>
        protected static TSqlObject GetTableFromColumn(TSqlObject sqlObj, QuerySpecification query, ColumnReferenceExpression column)
        {
            var tables = new List<NamedTableReference>();

            var namedTableVisitor = new NamedTableReferenceVisitor();
            query.FromClause.Accept(namedTableVisitor);

            if (column.MultiPartIdentifier.Identifiers.Count == 2)
            {
                tables.AddRange(namedTableVisitor.Statements.Where(x => x.Alias?.Value == column.MultiPartIdentifier.Identifiers[0].Value));
            }
            else
            {
                // they did NOT use a two part name, so logic dictates that this column SHOULD only appear once in the list of tables, but we will have to search all of the tables.
                tables.AddRange(namedTableVisitor.Statements);
            }

            var referencedTables = sqlObj.GetReferenced().Where(x => x.ObjectType == Table.TypeClass && tables.Any(t => x.Name.CompareTo(t.SchemaObject.Identifiers) >= 5));

            foreach (var referencedTable in referencedTables)
            {
                var fullColumnName = referencedTable.Name + ".[" + column.MultiPartIdentifier.Identifiers.Last().Value + "]";
                var retColumn = referencedTable.GetReferencedRelationshipInstances(Table.Columns).FirstOrDefault(p => Comparer.Equals(p.ObjectName.ToString(), fullColumnName));

                if (retColumn != null) { return referencedTable; }
            }

            return null;
        }
    }
}