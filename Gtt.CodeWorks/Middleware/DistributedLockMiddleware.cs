using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Gtt.CodeWorks.Middleware
{
    public class DistributedLockMiddleware<TRequest> : IServiceMiddleware
        where TRequest : BaseRequest, new()
    {
        private readonly IDistributedLockService _distributedLockService;
        private readonly ILogger _logger;
        private readonly Func<TRequest, CancellationToken, Task<string>> _createLockDelegate;
        private string _key = string.Empty;

        public DistributedLockMiddleware(
            IDistributedLockService distributedLockService,
            ILogger logger,
            Func<TRequest, CancellationToken, Task<string>> createLockDelegate)
        {
            _distributedLockService = distributedLockService;
            _logger = logger;
            _createLockDelegate = createLockDelegate;
        }

        public async Task<ServiceResponse> OnRequest<TReq>(IServiceInstance service, TReq request, CancellationToken cancellationToken)
            where TReq : BaseRequest, new()
        {
            object obj = request;
            TRequest tr = (TRequest)obj;
            _key = await _createLockDelegate(tr, cancellationToken);
            if (string.IsNullOrWhiteSpace(_key))
            {
                return this.ContinuePipeline();
            }

            _logger.LogTrace($"obtaining lock: key={_key}");
            var locked = await _distributedLockService.CreateLock(_key, cancellationToken);
            _logger.LogTrace($"Lock status key={_key}, status={locked}");
            if (locked == DistributedLockStatus.Locked)
            {
                return new ServiceResponse(new ResponseMetaData(
                        service,
                        ServiceResult.TransientError,
                        exceptionMessage: $"A lock already exists for key: {_key}")
                );
            }

            return this.ContinuePipeline();
        }

        public Task OnResponse<TReq, TRes>(
            IServiceInstance service,
            TReq request,
            ServiceResponse<TRes> response,
            CancellationToken cancellationToken) where TReq : BaseRequest, new() where TRes : new()
        {
            _logger.LogTrace($"releasing lock key={_key}");
            return Task.FromResult(
                _distributedLockService.ReleaseLock(_key, CancellationToken.None)
                );
        }

        public bool IgnoreExceptions => false;
        public bool SkipOnInternalCall => false;
    }
}
