using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.Utilities
{
    public static class StringExtensions
    {
        public static string NullIfEmpty(this string text)
        {
            return String.IsNullOrWhiteSpace(text) ? null : text;
        }
    }
}
