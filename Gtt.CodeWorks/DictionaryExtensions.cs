﻿using System;
using System.Collections;
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

        public static void AddOrAppendValue(this IDictionary<string, object> dict, string key, object value, bool forceArray = true)
        {
            if (dict == null) throw new ArgumentNullException(nameof(dict));
            var existing = dict.GetValueOrDefault(key);
            if (existing == null)
            {
                if (!forceArray)
                {
                    dict[key] = value;
                }
                else
                {
                    dict[key] = new List<object> { value };
                }
            }
            else
            {
                if (existing is ICollection coll)
                {
                    object[] newA = new object[coll.Count + 1];
                    coll.CopyTo(newA, 0);
                    newA[coll.Count] = value;
                    dict[key] = newA;
                }
                else
                {
                    dict[key] = new List<object> { existing, value };
                }
            }
        }
    }
}
