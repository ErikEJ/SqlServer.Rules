using System.Collections.Generic;

namespace SqlServer.Rules.Globals
{
    internal static class Constants
    {
        public const string RuleNameSpace = "SqlServer.Rules.";
        public const string Performance = "Performance";
        public const string Design = "Design";
        public const string Naming = "Naming";

        public static List<string> Aggregates = new List<string>
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
            "MAX"
        };

        public static List<string> DateParts = new List<string>
        {
            "year",
            "yy",
            "yyyy",
            "quarter",
            "qq",
            "q",
            "month",
            "mm",
            "m",
            "dayofyear",
            "dy",
            "y",
            "day",
            "dd",
            "d",
            "week",
            "wk",
            "ww",
            "weekday",
            "dw",
            "w",
            "hour",
            "hh",
            "minute",
            "mi",
            "n",
            "second",
            "ss",
            "s",
            "millisecond",
            "ms",
            "microsecond",
            "mcs",
            "nanosecond",
            "ns"
        };
    }
}
