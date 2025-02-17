using System;
using System.Globalization;
using System.Linq;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac
{
    public enum DataTypeViewType
    {
        Parameter,
        Declare,
        Column,
    }

    public class DataTypeView
    {
        public DataTypeView(TSqlObject column)
        {
            if (column.ObjectType != Column.TypeClass)
            {
                throw new ArgumentException("Invalid object type for the parameter. Should be of type Column.", nameof(column));
            }

            Name = column.Name.Parts.Last();
            Type = DataTypeViewType.Column;

            var dataType = column.GetReferenced(Column.DataType).FirstOrDefault();
            if (dataType != null)
            {
                var length = column.GetProperty<int>(Column.Length);
                DataType = dataType.Name.Parts.First();
                Length = length;
            }
        }

        public DataTypeView(string name, DataTypeReference dataType, DataTypeViewType type)
        {
            Name = name;
            DataType = dataType.Name.Identifiers.Last().Value;
            var length = (dataType as ParameterizedDataTypeReference)?.Parameters.FirstOrDefault()?.Value;
            Length = length?.ToUpperInvariant() == "MAX" ? -1 : Convert.ToInt32(length, CultureInfo.InvariantCulture);
            Type = type;
        }

        public string Name { get; set; }

        public string DataType { get; set; }

        public DataTypeViewType Type { get; set; }

        public int Length { get; set; }

        public override string ToString()
        {
            var len = string.Empty;
            if (Length == -1)
            {
                len = " (MAX)";
            }
            else if (Length > 0)
            {
                len = $" ({Length})";
            }

            return !string.IsNullOrEmpty(Name) ? $"{Type} {Name} {DataType}{len}" : base.ToString();
        }
    }
}
