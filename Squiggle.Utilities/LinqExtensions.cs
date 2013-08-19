using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.Utilities
{
    public static class LinqExtensions
    {
        public static bool In<T>(this T item, params T[] options)
        {
            bool result = options.Any(o=>o.Equals(item));
            return result;
        }

        public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
        {
            foreach (var item in items)
                action(item);
        }

        public static IEnumerable<T> Append<T>(this IEnumerable<T> items, T item)
        {
            return items.Concat(Enumerable.Repeat(item, 1));
        }

        public static string ToTraceString<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            string output = String.Join(", ", items.Select(i => i.Key + " = " + i.Value).ToArray());
            return output;
        }
    }
}
