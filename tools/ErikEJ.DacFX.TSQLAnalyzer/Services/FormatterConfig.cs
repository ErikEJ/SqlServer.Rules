using System.Globalization;
using EditorConfig.Core;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace ErikEJ.DacFX.TSQLAnalyzer.Services
{
    internal static class FormatterConfig
    {
        public static SqlScriptGeneratorOptions GetOptions(string scriptPath)
        {
            var parser = new EditorConfigParser();
            FileConfiguration rules = parser.Parse(scriptPath);

            var defaultOptions = new SqlScriptGeneratorOptions();

            return new SqlScriptGeneratorOptions()
            {
                AlignClauseBodies = GetValue(rules, "align_clause_bodies", defaultOptions.AlignClauseBodies),
                AlignColumnDefinitionFields = GetValue(rules, "align_column_definition_fields", defaultOptions.AlignColumnDefinitionFields),
                AllowExternalLanguagePaths = GetValue(rules, "allow_external_language_paths", defaultOptions.AllowExternalLanguagePaths),
                AlignSetClauseItem = GetValue(rules, "align_set_clause_item", defaultOptions.AlignSetClauseItem),
                AllowExternalLibraryPaths = GetValue(rules, "allow_external_library_paths", defaultOptions.AllowExternalLibraryPaths),
                AsKeywordOnOwnLine = GetValue(rules, "as_keyword_on_own_line", defaultOptions.AsKeywordOnOwnLine),
                IncludeSemicolons = GetValue(rules, "include_semicolons", defaultOptions.IncludeSemicolons),
                IndentSetClause = GetValue(rules, "indent_set_clause", defaultOptions.IndentSetClause),
                KeywordCasing = GetValue(rules, "keyword_casing", defaultOptions.KeywordCasing),
                IndentationSize = GetValue(rules, "indentation_size", defaultOptions.IndentationSize),
                IndentViewBody = GetValue(rules, "indent_view_body", defaultOptions.IndentViewBody),
                MultilineInsertSourcesList = GetValue(rules, "multiline_insert_sources_list", defaultOptions.MultilineInsertSourcesList),
                MultilineInsertTargetsList = GetValue(rules, "multiline_insert_targets_list", defaultOptions.MultilineInsertTargetsList),
                MultilineSelectElementsList = GetValue(rules, "multiline_select_elements_list", defaultOptions.MultilineSelectElementsList),
                MultilineSetClauseItems = GetValue(rules, "multiline_set_clause_items", defaultOptions.MultilineSetClauseItems),
                MultilineViewColumnsList = GetValue(rules, "multiline_view_columns_list", defaultOptions.MultilineViewColumnsList),
                MultilineWherePredicatesList = GetValue(rules, "multiline_where_predicates_list", defaultOptions.MultilineWherePredicatesList),
                NewLineBeforeCloseParenthesisInMultilineList = GetValue(rules, "new_line_before_close_parenthesis_in_multiline_list", defaultOptions.NewLineBeforeCloseParenthesisInMultilineList),
                NewLineBeforeFromClause = GetValue(rules, "new_line_before_from_clause", defaultOptions.NewLineBeforeFromClause),
                NewLineBeforeGroupByClause = GetValue(rules, "new_line_before_group_by_clause", defaultOptions.NewLineBeforeGroupByClause),
                NewLineBeforeHavingClause = GetValue(rules, "new_line_before_having_clause", defaultOptions.NewLineBeforeHavingClause),
                NewLineBeforeJoinClause = GetValue(rules, "new_line_before_join_clause", defaultOptions.NewLineBeforeJoinClause),
                NewLineBeforeOffsetClause = GetValue(rules, "new_line_before_offset_clause", defaultOptions.NewLineBeforeOffsetClause),
                NewLineBeforeOpenParenthesisInMultilineList = GetValue(rules, "new_line_before_open_parenthesis_in_multiline_list", defaultOptions.NewLineBeforeOpenParenthesisInMultilineList),
                NewLineBeforeOrderByClause = GetValue(rules, "new_line_before_order_by_clause", defaultOptions.NewLineBeforeOrderByClause),
                NewLineBeforeOutputClause = GetValue(rules, "new_line_before_output_clause", defaultOptions.NewLineBeforeOutputClause),
                NewLineBeforeWhereClause = GetValue(rules, "new_line_before_where_clause", defaultOptions.NewLineBeforeWhereClause),
                NewLineBeforeWindowClause = GetValue(rules, "new_line_before_window_clause", defaultOptions.NewLineBeforeWindowClause),
                NewlineFormattedCheckConstraint = GetValue(rules, "newline_formatted_check_constraint", defaultOptions.NewlineFormattedCheckConstraint),
                NewLineFormattedIndexDefinition = GetValue(rules, "newline_formatted_index_definition", defaultOptions.NewLineFormattedIndexDefinition),
                NumNewlinesAfterStatement = GetValue(rules, "num_newlines_after_statement", defaultOptions.NumNewlinesAfterStatement),
                SpaceBetweenDataTypeAndParameters = GetValue(rules, "space_between_data_type_and_parameters", defaultOptions.SpaceBetweenDataTypeAndParameters),
                SpaceBetweenParametersInDataType = GetValue(rules, "space_between_parameters_in_data_type", defaultOptions.SpaceBetweenParametersInDataType),
                SqlEngineType = GetValue(rules, "sql_engine_type", defaultOptions.SqlEngineType),
                SqlVersion = GetValue(rules, "sql_version", SqlVersion.Sql160),
            };
        }

        private static T GetValue<T>(FileConfiguration rule, string property, T defaultValue)
        {
            if (rule.Properties.TryGetValue(property, out var value))
            {
                if (defaultValue is KeywordCasing && Enum.TryParse(value, true, out KeywordCasing casing))
                {
                    return (T)((object)casing);
                }

                if (defaultValue is SqlVersion && Enum.TryParse(value, true, out SqlVersion version))
                {
                    return (T)((object)version);
                }

                if (defaultValue is SqlEngineType && Enum.TryParse(value, true, out SqlEngineType engine))
                {
                    return (T)((object)engine);
                }

                return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
            }

            return defaultValue;
        }
    }
}
