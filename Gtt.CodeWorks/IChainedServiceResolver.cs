using System;
using System.Collections.Generic;
using System.Linq;

namespace Gtt.CodeWorks
{
    public interface IChainedServiceResolver
    {
        void AddChainedService(IServiceInstance serviceInstance);
        IEnumerable<IServiceInstance> AllInstances();
    }

    public class DefaultChainedServiceResolver : IChainedServiceResolver
    {
        private Guid _id = Guid.NewGuid();

        private readonly HashSet<IServiceInstance> _services = new HashSet<IServiceInstance>();
        public void AddChainedService(IServiceInstance serviceInstance)
        {
            _services.Add(serviceInstance);
        }

        public IEnumerable<IServiceInstance> AllInstances()
        {
            return _services.OrderBy(x => x.FullName).ToList();
        }
    }
}