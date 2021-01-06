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

        public static void AddOrAppendValue(this IDictionary<string, string[]> dict, string key, string value)
        {
            if (dict == null) throw new ArgumentNullException(nameof(dict));
            var existing = dict.GetValueOrDefault(key);
            if (existing == null)
            {
                dict[key] = new[] { value };
            }
            else
            {
                string[] newA = new string[existing.Length + 1];
                Array.Copy(existing, newA, existing.Length);
                newA[newA.Length - 1] = value;
                dict[key] = newA;
            }
        }
    }
}
