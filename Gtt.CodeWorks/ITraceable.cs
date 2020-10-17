using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Gtt.CodeWorks
{
    public interface ITraceable
    {
        Guid CorrelationId { get; set; }
    }

    public static class TraceableExtensions
    {
        public static void SyncCorrelationIds(this ITraceable req)
        {
            var parentCorrelationId = req.CorrelationId;
            List<ITraceable> visited = new List<ITraceable>();
            int iterations = 0;

            SetCorrelationIdsRecursively(req, parentCorrelationId, visited, ref iterations);
        }

        private static void SetCorrelationIdsRecursively(object root, Guid correlationId, List<ITraceable> visited, ref int iterations)
        {
            iterations++;

            if (iterations > 1000)
            {
                throw new Exception("Sync Correlation Max Iterations of 1000 reached");
            }

            var t = root.GetType();
            if (root is ITraceable rootRequest)
            {
                rootRequest.CorrelationId = correlationId;
                visited.Add(rootRequest);
            }

            foreach (PropertyInfo p in t.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (p.PropertyType.GetInterfaces().Contains(typeof(ITraceable)))
                {
                    if (p.GetValue(root) is ITraceable child)
                    {
                        if (!visited.Contains(child))
                        {
                            SetCorrelationIdsRecursively(child, correlationId, visited, ref iterations);
                        }
                    }
                }

                if (p.PropertyType.GetInterfaces().Contains(typeof(IEnumerable)))
                {
                    var val = p.GetValue(root);
                    if (val is IEnumerable coll && !(val is string))
                    {
                        foreach (var c in coll)
                        {
                            if (c == null) continue;
                            if (!visited.Contains(c))
                            {
                                SetCorrelationIdsRecursively(c, correlationId, visited, ref iterations);
                            }
                        }
                    }
                }
            }
        }
    }
}