using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace SqlAnalyzerSsms.Options
{
    /// <summary>
    /// Provides a dropdown list of SQL engine version values for the Visual Studio options property grid.
    /// </summary>
    internal sealed class SqlEngineVersionConverter : StringConverter
    {
        private static readonly Dictionary<string, string> VersionToLabel = new Dictionary<string, string>
        {
            { "Sql170", "Sql170 - SQL Server 2025 and Managed Instance" },
            { "Sql160", "Sql160 - SQL Server 2022" },
            { "SqlAzure", "SqlAzure - Azure SQL Database" },
            { "SqlDw", "SqlDw - Microsoft Azure SQL Data Warehouse" },
            { "SqlServerless", "SqlServerless - Azure Synapse Analytics Serverless SQL Pool" },
            { "SqlDwUnified", "SqlDwUnified - Fabric Data Warehouse" },
            { "Sql150", "Sql150 - SQL Server 2019" },
            { "Sql140", "Sql140 - SQL Server 2017" },
            { "Sql130", "Sql130 - SQL Server 2016" },
            { "Sql120", "Sql120 - SQL Server 2014" },
            { "Sql110", "Sql110 - SQL Server 2012" },
            { "Sql100", "Sql100 - SQL Server 2008" },
            { "Sql90", "Sql90 - SQL Server 2005" },
        };

        private static readonly Dictionary<string, string> LabelToVersion = new Dictionary<string, string>();

        static SqlEngineVersionConverter()
        {
            foreach (var kvp in VersionToLabel)
            {
                LabelToVersion[kvp.Value] = kvp.Key;
            }
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext? context) => true;

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext? context) => false;

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext? context)
            => new StandardValuesCollection(new[]
            {
                "Sql170",
                "Sql160",
                "SqlAzure",
                "SqlDw",
                "SqlServerless",
                "SqlDwUnified",
                "Sql150",
                "Sql140",
                "Sql130",
                "Sql120",
                "Sql110",
                "Sql100",
                "Sql90",
            });

        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, System.Type destinationType)
        {
            if (destinationType == typeof(string) && value is string version)
            {
                if (VersionToLabel.TryGetValue(version, out var label))
                {
                    return label;
                }
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is string input)
            {
                // Accept a full label like "Sql160 - SQL Server 2022" and return the code
                if (LabelToVersion.TryGetValue(input, out var code))
                {
                    return code;
                }

                // Accept a bare version code directly
                if (VersionToLabel.ContainsKey(input))
                {
                    return input;
                }
            }

            return base.ConvertFrom(context, culture, value);
        }
    }
}
