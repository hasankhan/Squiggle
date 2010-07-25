using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.Chat
{
    public static class LinqExtensions
    {
        public static bool In<T>(this T item, params T[] options)
        {
            bool result = options.Any(o=>o.Equals(item));
            return result;
        }
    }
}
