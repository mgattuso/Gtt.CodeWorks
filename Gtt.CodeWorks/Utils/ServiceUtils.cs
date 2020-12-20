using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gtt.CodeWorks.Utils
{
    public static class ServiceUtils
    {
        private static IEnumerable<Type> GetConcreteInstancesOf<T>()
        {
            var a = AppDomain.CurrentDomain.GetAssemblies();
            var cls = a.SelectMany(x => x.GetTypes()).Where(p => typeof(T).IsAssignableFrom(p) && !p.IsAbstract);
            return cls;
        }
    }
}
