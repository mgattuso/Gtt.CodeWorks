using System;
using System.Collections.Generic;
using System.Text;

namespace Gtt.CodeWorks
{
    public static class DictionaryExtensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
        {
            if (dict == null) return default(TValue);
            return dict.TryGetValue(key, out var val) ? val : default(TValue);
        }
    }
}
