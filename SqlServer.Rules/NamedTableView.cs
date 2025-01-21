using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServer.Dac;

namespace SqlServer.Rules
{
    public class NamedTableView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NamedTableView"/> class.
        /// </summary>
        /// <param name="namedTable">The named table.</param>
        public NamedTableView(NamedTableReference namedTable)
        {
            Name = namedTable.GetName("dbo");
            if (namedTable.Alias != null && !HasAlias(namedTable.Alias.Value))
            {
                Aliases.Add(namedTable.Alias.Value);
            }
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

#pragma warning disable CA1002 // Do not expose generic lists
        public List<string> Aliases { get; private set; } = new List<string>();
#pragma warning restore CA1002 // Do not expose generic lists

        private static readonly char[] Separator = new[] { '.' };

        public ObjectIdentifier NameToId()
        {
            return new ObjectIdentifier((Name ?? string.Empty).Split(Separator, StringSplitOptions.RemoveEmptyEntries));
        }

        /// <summary>
        /// Determines whether the specified alias has alias.
        /// </summary>
        /// <param name="alias">The alias.</param>
        /// <returns>
        ///   <c>true</c> if the specified alias has alias; otherwise, <c>false</c>.
        /// </returns>
        public bool HasAlias(string alias)
        {
            return Aliases.Any(a => a.StringEquals(alias));
        }

        /// <summary>
        /// Determines whether the specified identifier is match.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>
        ///   <c>true</c> if the specified identifier is match; otherwise, <c>false</c>.
        /// </returns>
        public bool IsMatch(ObjectIdentifier id)
        {
            var tableNameOrAlias = new ObjectIdentifier(id.Parts.Take(id.Parts.Count - 1));

            return NameToId().CompareTo(tableNameOrAlias) >= 5
                || HasAlias(tableNameOrAlias.Parts.First());
        }

        /// <summary>
        /// Determines whether the specified <see cref="object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (!(obj is NamedTableView tmp))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(Name))
            {
                return Name.StringEquals(tmp.Name);
            }

            return false;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            if (!string.IsNullOrWhiteSpace(Name))
            {
                return Name.GetHashCode(StringComparison.OrdinalIgnoreCase);
            }

            return base.GetHashCode();
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }
    }
}
