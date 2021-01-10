using System;
using System.Collections.Generic;
using System.Text;

namespace Gtt.CodeWorks
{
    public static class StringExtensions
    {
        public static string ToCamelCase(this string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return str;
            return str[0].ToString().ToLowerInvariant() + str.Substring(1, str.Length - 1);
        }
    }
}
