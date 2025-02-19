namespace ErikEJ.DacFX.TSQLAnalyzer.Extensions
{
    internal static class RulesInfo
    {
        public static readonly Dictionary<string, (string, string, string)> MicrosoftRules = new()
        {
            {
                "SR0001",  ("https://learn.microsoft.com/sql/tools/sql-database-projects/concepts/sql-code-analysis/t-sql-design-issues#sr0001-avoid-select--in-stored-procedures-views-and-table-valued-functions",
                "Avoid SELECT * in stored procedures, views, and table-valued functions", "Design")
            },
            {
                "SR0008",  ("https://learn.microsoft.com/sql/tools/sql-database-projects/concepts/sql-code-analysis/t-sql-design-issues#sr0008-consider-using-scope_identity-instead-of-identity",
                "Consider using SCOPE_IDENTITY instead of @@IDENTITY", "Design")
            },
            {
                "SR0009",  ("https://learn.microsoft.com/sql/tools/sql-database-projects/concepts/sql-code-analysis/t-sql-design-issues#sr0009-avoid-using-types-of-variable-length-that-are-size-1-or-2",
                "Avoid using types of variable length that are size 1 or 2", "Design")
            },
            {
                "SR0010",  ("https://learn.microsoft.com/sql/tools/sql-database-projects/concepts/sql-code-analysis/t-sql-design-issues#sr0010-avoid-using-deprecated-syntax-when-you-join-tables-or-views",
                "Avoid using deprecated syntax when you join tables or views", "Design")
            },
            {
                "SR0013",  ("https://learn.microsoft.com/sql/tools/sql-database-projects/concepts/sql-code-analysis/t-sql-design-issues#sr0013-output-parameter-parameter-isnt-populated-in-all-code-paths",
                "Output parameter (parameter) isn't populated in all code paths", "Design")
            },
            {
                "SR0014",  ("https://learn.microsoft.com/sql/tools/sql-database-projects/concepts/sql-code-analysis/t-sql-design-issues#sr0014-data-loss-might-occur-when-casting-from-type1-to-type2",
                "Data loss might occur when casting from {Type1} to {Type2}", "Design")
            },
            {
                "SR0011",  ("https://learn.microsoft.com/sql/tools/sql-database-projects/concepts/sql-code-analysis/t-sql-naming-issues#sr0011-avoid-using-special-characters-in-object-names",
                "Avoid using special characters in object names", "Naming")
            },
            {
                "SR0012",  ("https://learn.microsoft.com/sql/tools/sql-database-projects/concepts/sql-code-analysis/t-sql-naming-issues#sr0012-avoid-using-reserved-words-for-type-names",
                "Avoid using reserved words for type names", "Naming")
            },
            {
                "SR0016",  ("https://learn.microsoft.com/sql/tools/sql-database-projects/concepts/sql-code-analysis/t-sql-naming-issues#sr0016-avoid-using-sp_-as-a-prefix-for-stored-procedures",
                "Avoid using sp_ as a prefix for stored procedures", "Naming")
            },
            {
                "SR0004",  ("https://learn.microsoft.com/sql/tools/sql-database-projects/concepts/sql-code-analysis/t-sql-performance-issues#sr0004-avoid-using-columns-that-dont-have-indexes-as-test-expressions-in-in-predicates",
                "Avoid using columns that don't have indexes as test expressions in IN predicates", "Performance")
            },
            {
                "SR0005",  ("https://learn.microsoft.com/sql/tools/sql-database-projects/concepts/sql-code-analysis/t-sql-performance-issues#sr0005-avoid-using-patterns-that-start-with--in-like-predicates",
                "Avoid using patterns that start with \"%\" in LIKE predicates", "Performance")
            },
            {
                "SR0006",  ("https://learn.microsoft.com/sql/tools/sql-database-projects/concepts/sql-code-analysis/t-sql-performance-issues#sr0006-move-a-column-reference-to-one-side-of-a-comparison-operator-to-use-a-column-index",
                "Move a column reference to one side of a comparison operator to use a column index", "Performance")
            },
            {
                "SR0007",  ("https://learn.microsoft.com/sql/tools/sql-database-projects/concepts/sql-code-analysis/t-sql-performance-issues#sr0007-use-isnullcolumn-default_value-on-nullable-columns-in-expressions",
                "Use ISNULL(column, default_value) on nullable columns in expressions", "Performance")
            },
            {
                "SR0015",  ("https://learn.microsoft.com/sql/tools/sql-database-projects/concepts/sql-code-analysis/t-sql-performance-issues#sr0015-extract-deterministic-function-calls-from-where-predicates",
                "Extract deterministic function calls from WHERE predicates", "Performance")
            },
        };
    }
}
