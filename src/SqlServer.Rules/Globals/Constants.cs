using System.Collections.Generic;

namespace SqlServer.Rules.Globals
{
    public static class Constants
    {
        public const string RuleNameSpace = "SqlServer.Rules.";
        public const string Performance = "Performance";
        public const string Design = "Design";
        public const string Naming = "Naming";

#pragma warning disable CA1002 // Do not expose generic lists
        public static List<string> Aggregates { get; private set; } = new List<string>
        {
            "AVG",
            "MIN",
            "CHECKSUM_AGG",
            "SUM",
            "COUNT",
            "STDEV",
            "COUNT_BIG",
            "STDEVP",
            "GROUPING",
            "VAR",
            "GROUPING_ID",
            "VARP",
            "MAX",
        };

        public static List<string> DateParts { get; private set; } = new List<string>
        {
            "YEAR",
            "YY",
            "YYYY",
            "QUARTER",
            "QQ",
            "Q",
            "MONTH",
            "MM",
            "M",
            "DAYOFYEAR",
            "DY",
            "Y",
            "DAY",
            "DD",
            "D",
            "WEEK",
            "WK",
            "WW",
            "WEEKDAY",
            "DW",
            "W",
            "HOUR",
            "HH",
            "MINUTE",
            "MI",
            "N",
            "SECOND",
            "SS",
            "S",
            "MILLISECOND",
            "MS",
            "MICROSECOND",
            "MCS",
            "NANOSECOND",
            "NS",
        };
    }
}
#pragma warning restore CA1002 // Do not expose generic lists