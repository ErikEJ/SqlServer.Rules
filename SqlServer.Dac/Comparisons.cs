using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServer.Dac
{
    public static class Comparisons
    {
        public static int CompareTo(this ObjectIdentifier identifier, MultiPartIdentifier mpIdentifier)
        {
            return CompareIdentifiers(identifier, mpIdentifier.Identifiers.Select(x => x.Value));
        }

        public static int CompareTo(this ObjectIdentifier identifier, IList<Identifier> identifiers)
        {
            return CompareIdentifiers(identifier, identifiers.Select(x => x.Value));
        }

        public static int CompareTo(this MultiPartIdentifier identifier, ObjectIdentifier oIdentifier)
        {
            return CompareIdentifiers(oIdentifier, identifier.Identifiers.Select(x => x.Value));
        }

        public static int CompareTo(this ObjectIdentifier identifier, ObjectIdentifier identifier2)
        {
            return CompareIdentifiers(identifier, identifier2.Parts);
        }

        public static int CompareTo(this ObjectIdentifier identifier, IList<string> identifiers)
        {
            return CompareIdentifiers(identifier, identifiers);
        }

        /// <summary>
        /// Compares the identifiers. Will return the cumulative values when the specified parts match:
        /// Server Name     : 0  -- should not happen
        /// Database Name   : 1  -- this should only happen when it is a referenced db
        /// Schema Name     : 3
        /// Object Name     : 5
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        /// <param name="identifiers">The identifiers.</param>
        /// <remarks>If we get AT least a score of 5 we must assume it is a match. Then we have at least matched 
        /// on the table name. A score of 8 means we have also matched the schema. A score of 4 or below means we 
        /// definitely did not match on object name.</remarks>
        /// <returns></returns>
        private static int CompareIdentifiers(this ObjectIdentifier identifier, IEnumerable<string> identifiers)
        {
            var ret = 0;

            var left = PadNames(identifier.Parts);
            var right = PadNames(identifiers);

            for (var i = 1; i < 4; i++)
            {
                if (!string.IsNullOrWhiteSpace(left[i])
                    && !string.IsNullOrWhiteSpace(right[i])
                    && left[i].StringEquals(right[i]))
                {
                    switch (i)
                    {
                        case 1:
                            ret += 1;
                            break;
                        case 2:
                            ret += 3;
                            break;
                        case 3:
                            ret += 5;
                            break;
                    }
                }
            }

            return ret;
        }

        private static List<string> PadNames(IEnumerable<string> nameParts)
        {
            var ret = new List<string>(nameParts);
            for (var i = ret.Count; i < 4; i++)
            {
                ret.Insert(0, string.Empty);
            }

            return ret;
        }
    }
}
