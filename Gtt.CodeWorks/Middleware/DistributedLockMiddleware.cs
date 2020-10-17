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
        private readonly Func<TRequest, CancellationToken, Task<string>> _createLockDelegate;
        private string _key = string.Empty;
        private static readonly HashSet<string> HashSet = new HashSet<string>();
        private readonly object _lock = new object();
        public DistributedLockMiddleware(Func<TRequest, CancellationToken, Task<string>> createLockDelegate)
        {
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

            bool locked;

            lock (_lock)
            {
                locked = HashSet.Contains(_key);
                if (!locked)
                {
                    HashSet.Add(_key);
                }
            }

            if (locked)
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
            lock (_lock)
            {
                HashSet.Remove(_key);
            }

            return Task.CompletedTask;
        }

        public bool IgnoreExceptions => false;
    }
}
