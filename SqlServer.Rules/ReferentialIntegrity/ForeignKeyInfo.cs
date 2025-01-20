using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dac.Model;
using SqlServer.Dac;

namespace SqlServer.Rules.ReferentialIntegrity
{
    /// <summary>
    ///
    /// </summary>
    public class ForeignKeyInfo
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the table.
        /// </summary>
        /// <value>
        /// The name of the table.
        /// </value>
        public ObjectIdentifier TableName { get; set; }

        /// <summary>
        /// Converts to tablename.
        /// </summary>
        /// <value>
        /// The name of to table.
        /// </value>
        public ObjectIdentifier ToTableName { get; set; }

        /// <summary>
        /// Gets or sets the column names.
        /// </summary>
        /// <value>
        /// The column names.
        /// </value>
#pragma warning disable CA2227 // Collection properties should be read only
        public IList<ObjectIdentifier> ColumnNames { get; set; }

        /// <summary>
        /// Converts to columnnames.
        /// </summary>
        /// <value>
        /// To column names.
        /// </value>
        public IList<ObjectIdentifier> ToColumnNames { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var cols = new List<string>();
            var toCols = new List<string>();
            foreach (var col in ColumnNames)
            {
                cols.Add(col.Parts.Last());
            }

            foreach (var col in ToColumnNames)
            {
                toCols.Add(col.Parts.Last());
            }

            // CONSTRAINT [FK_Table2_ToTable1] FOREIGN KEY ([Tbl1Id], [Tbl1Id2]) REFERENCES [Table1]([Table1Id], [Table1Id2])
            return $"CONSTRAINT {Name} FOREIGN KEY {TableName.GetName()} ({string.Join(", ", cols)}) REFERENCES  {ToTableName.GetName()} ({string.Join(", ", toCols)})";
        }
    }
}