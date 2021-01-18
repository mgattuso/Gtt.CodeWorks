using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gtt.CodeWorks.Middleware
{
    public class RateLimiterMiddleware : IServiceMiddleware
    {
        private readonly IRateLimiter _rateLimiter;

        public RateLimiterMiddleware(IRateLimiter rateLimiter)
        {
            _rateLimiter = rateLimiter;
        }
        public async Task<ServiceResponse> OnRequest<TReq>(IServiceInstance service, TReq request, CancellationToken cancellationToken)
            where TReq : BaseRequest, new()
        {
            RateLimitStatus rateLimitResult = await _rateLimiter.EnterService(request);
            if (rateLimitResult == RateLimitStatus.HardLimit)
            {
                return new ServiceResponse(new ResponseMetaData(
                    service,
                    ServiceResult.RateLimited));
            }

            return this.ContinuePipeline();
        }

        public Task OnResponse<TReq, TRes>(IServiceInstance service, TReq request, ServiceResponse<TRes> response, CancellationToken cancellationToken) where TReq : BaseRequest, new() where TRes : new()
        {
            return _rateLimiter.LeaveService(request);
        }

        public bool IgnoreExceptions => true;
        public bool SkipOnInternalCall => true;
    }
}
