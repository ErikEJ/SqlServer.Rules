using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlServer.Dac
{
    public static class Misc
    {
        private static readonly StringComparer Comparer = StringComparer.OrdinalIgnoreCase;

        public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = value;
            }
            else
            {
                dictionary.Add(key, value);
            }
        }

        public static IDictionary<TKey, TValue> AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dic1, IDictionary<TKey, TValue> dic2)
        {
            foreach (var item in dic2)
            {
                dic1.AddOrUpdate(item.Key, item.Value);
            }

            return dic1;
        }

        public static void RemoveAll<TKey, TValue>(this IDictionary<TKey, TValue> dict, Func<TKey, TValue, bool> match)
        {
            foreach (var key in dict.Keys.ToArray().Where(key => match(key, dict[key])))
            {
                dict.Remove(key);
            }
        }

        public static void RemoveAll<T>(this IList<T> list, Func<T, bool> match)
        {
            for (var i = list.Count - 1; i >= 0; i--)
            {
                var item = list[i];
                if (match(item))
                {
                    list.Remove(item);
                }
            }
        }

        public static bool StringEquals(this object value1, object value2)
        {
            if (value1 == null || value2 == null)
            {
                return false;
            }

            return Comparer.Equals(value1, value2);
        }

#if NETFRAMEWORK
        public static bool Contains(this string str, char value, StringComparison comparison)
        {
            return str.IndexOf(new string(value, 1), comparison) >= 0;
        }

        public static bool Contains(this string str, string value, StringComparison comparison)
        {
            return str.IndexOf(value, comparison) >= 0;
        }

        public static bool StartsWith(this string str, char value)
        {
            return str.Length > 0 && str[0] == value;
        }

        public static string Replace(this string str, string find, string replace, StringComparison comparison)
        {
            var index = str.IndexOf(find, comparison);

            while (index >= 0)
            {
                str = str.Remove(index, find.Length).Insert(index, replace);
                index = str.IndexOf(find, index + replace.Length, comparison);
            }

            return str;
        }

        public static int GetHashCode(this string str, StringComparison comparison)
        {
            return GetComparer(comparison).GetHashCode(str);
        }

        private static StringComparer GetComparer(this StringComparison comparison)
        {
            return comparison switch
            {
                StringComparison.CurrentCulture => StringComparer.CurrentCulture,
                StringComparison.CurrentCultureIgnoreCase => StringComparer.CurrentCultureIgnoreCase,
                StringComparison.InvariantCulture => StringComparer.InvariantCulture,
                StringComparison.InvariantCultureIgnoreCase => StringComparer.InvariantCultureIgnoreCase,
                StringComparison.Ordinal => StringComparer.Ordinal,
                StringComparison.OrdinalIgnoreCase => StringComparer.OrdinalIgnoreCase,
                _ => throw new ArgumentOutOfRangeException(nameof(comparison), comparison, null),
            };
        }
#endif
    }
}