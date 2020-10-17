using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gtt.CodeWorks.Middleware
{
    public class DistributedLockMiddleware<TRequest> : IServiceMiddleware
        where TRequest : BaseRequest, new()
    {
        private readonly IDistributedLockService _distributedLockService;
        private readonly Func<TRequest, CancellationToken, Task<string>> _createLockDelegate;
        private string _key = string.Empty;

        public DistributedLockMiddleware(
            IDistributedLockService distributedLockService,
            Func<TRequest, CancellationToken, Task<string>> createLockDelegate)
        {
            _distributedLockService = distributedLockService;
            _createLockDelegate = createLockDelegate;
        }

        public async Task<ServiceResponse> OnRequest<TReq>(IServiceInstance service, TReq request, CancellationToken cancellationToken)
            where TReq : BaseRequest, new()
        {
            object obj = request;
            TRequest tr = (TRequest)obj;
            _key = await _createLockDelegate(tr, cancellationToken);
            if (string.IsNullOrEmpty(_key))
            {
                return this.ContinuePipeline();
            }

            var locked = await _distributedLockService.CreateLock(_key, cancellationToken);

            if (locked == DistributedLockStatus.Locked)
            {
                return new ServiceResponse(new ResponseMetaData(ServiceResult.TransientError,
                    service.CorrelationId,
                    service.StartTime, $"Distributed lock exists for key \"{_key}\""
                    ));
            }

            return this.ContinuePipeline();
        }

        public Task OnResponse<TReq, TRes>(IServiceInstance service, TReq request, ServiceResponse<TRes> response) where TReq : BaseRequest, new() where TRes : new()
        {
            return Task.FromResult(
                _distributedLockService.ReleaseLock(_key, CancellationToken.None)
                );
        }

        public bool IgnoreExceptions => false;
    }
}
