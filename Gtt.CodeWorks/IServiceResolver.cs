using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gtt.CodeWorks
{
    public interface IServiceResolver
    {
        public IServiceInstance GetInstanceByName(string name);
    }

    public class ServiceResolver : IServiceResolver
    {
        private readonly IServiceInstance[] _instances;

        public ServiceResolver(IEnumerable<IServiceInstance> instances)
        {
            _instances = instances?.ToArray() ?? new IServiceInstance[0];
        }

        public IServiceInstance GetInstanceByName(string name)
        {
            var matchingInstances =
                _instances.Where(x => x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase)).ToList();

            if (matchingInstances.Count == 0)
            {
                return null;
            }

            if (matchingInstances.Count > 1)
            {
                throw new MultipleMatchingServicesException();
            }

            return matchingInstances[0];
        }
    }
}
