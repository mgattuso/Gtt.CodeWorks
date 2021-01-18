using System;
using System.Collections.Generic;

namespace Gtt.CodeWorks
{
    public interface IChainedServiceResolver
    {
        void AddChainedService(IServiceInstance serviceInstance);
        IEnumerable<IServiceInstance> AllInstances();
        Guid Id { get; }
        void AddCallToChain(IServiceInstance baseServiceInstance);
        IEnumerable<IServiceInstance> GetCurrentCallChain();
    }
}