using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gtt.CodeWorks
{
    public abstract class BaseReadService<TRequest, TResponse> : BaseServiceInstance<TRequest, TResponse>
        where TRequest : BaseRequest, new()
        where TResponse : new()
    {
        protected BaseReadService(CoreDependencies coreDependencies) : base(coreDependencies)
        {
        }

        protected override Task<string> CreateDistributedLockKey(TRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(string.Empty);
        }
    }
}
