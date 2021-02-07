using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Gtt.CodeWorks.Tokenizer
{
    public static class TokenizeModel
    {
        public static async Task<Dictionary<int, TokenTracker>> CollectTokenizablesAll(object obj)
        {
            await Task.CompletedTask;
            int iterations = 0;
            Dictionary<int, TokenTracker> trackers = new Dictionary<int, TokenTracker>();
            List<object> visited = new List<object>();
            CollectTokenizableProperties(obj, trackers, visited, ref iterations);
            return trackers;
        }

        private static void CollectTokenizableProperties(
            object root,
            Dictionary<int, TokenTracker> trackers,
            List<object> visited,
            ref int iterations)
        {
            iterations++;

            if (iterations > 1000)
            {
                throw new Exception("CollectTokenizableProperties Max Iterations of 1000 reached");
            }

            var t = root.GetType();

            if (t == typeof(string))
            {
                return;
            }

            foreach (PropertyInfo p in t.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (p.PropertyType == typeof(string))
                {
                    continue;
                }

                if (p.PropertyType.GetInterfaces().Contains(typeof(IEnumerable)))
                {
                    if (p.GetValue(root) is IEnumerable coll)
                    {
                        foreach (var c in coll)
                        {
                            if (c == null) continue;

                            if (!visited.Contains(c))
                            {
                                visited.Add(c);
                                CollectTokenizableProperties(c, trackers, visited, ref iterations);
                            }
                        }
                    }
                }
                else
                {
                    var child = p.GetValue(root);

                    if (child != null && !visited.Contains(child))
                    {
                        visited.Add(child);

                        if (p.PropertyType.GetInterfaces().Contains(typeof(ITokenizable)))
                        {
                            var count = trackers.Count;
                            trackers[count + 1] = new TokenTracker
                            {
                                Correlation = count + 1,
                                Property = p,
                                Value = (ITokenizable)child,
                                Root = root,
                                SensitiveAttribute = p.GetCustomAttributes<SensitiveAttribute>().FirstOrDefault()
                            };
                        }
                        else
                        {
                            CollectTokenizableProperties(child, trackers, visited, ref iterations);
                        }
                    }

                }
            }
        }
    }
}