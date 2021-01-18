using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Gtt.CodeWorks
{
    public class DefaultChainedServiceResolver : IChainedServiceResolver
    {
        private readonly ILogger<DefaultChainedServiceResolver> _logger;
        private readonly HashSet<IServiceInstance> _services = new HashSet<IServiceInstance>();
        private readonly IList<IServiceInstance> _stack = new List<IServiceInstance>();

        public DefaultChainedServiceResolver(ILogger<DefaultChainedServiceResolver> logger)
        {
            _logger = logger;
        }
        public void AddChainedService(IServiceInstance serviceInstance)
        {
            _logger.LogTrace($"Adding {serviceInstance.FullName} to chained service resolve {Id}");
            _services.Add(serviceInstance);
        }

        public IEnumerable<IServiceInstance> AllInstances()
        {
            return _services.OrderBy(x => x.FullName).ToList();
        }

        public Guid Id { get; } = Guid.NewGuid();
        public void AddCallToChain(IServiceInstance instance)
        {
            _stack.Add(instance);
        }

        public IEnumerable<IServiceInstance> GetCurrentCallChain()
        {
            return _stack.ToList();
        }
    }
}