using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gtt.CodeWorks.Middleware
{
    public class LoggingMiddleware : IServiceMiddleware
    {
        private readonly IServiceLogger _logger;

        public LoggingMiddleware(IServiceLogger logger)
        {
            _logger = logger;
        }
        public async Task<ServiceResponse> OnRequest<TReq>(IServiceInstance service, TReq request, CancellationToken cancellationToken)
            where TReq : BaseRequest, new()
        {
             await Task.FromResult(_logger.LogRequest(request.CorrelationId, service.Name, request));
             return this.ContinuePipeline();
        }

        public Task OnResponse<TReq, TRes>(IServiceInstance service, TReq request, ServiceResponse<TRes> response)
            where TReq : BaseRequest, new()
            where TRes : new()
        {
            return Task.FromResult(_logger.LogResponse(request.CorrelationId, service.Name, response));
        }

        public bool IgnoreExceptions => true;
    }
}
