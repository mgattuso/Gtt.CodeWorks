using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Gtt.CodeWorks
{
    public class ServiceLogger : IServiceLogger
    {
        private readonly ILogObjectSerializer _logObjectSerializer;
        private readonly ILogger<ServiceLogger> _logger;

        public ServiceLogger(ILogObjectSerializer logObjectSerializer, ILogger<ServiceLogger> logger)
        {
            _logObjectSerializer = logObjectSerializer;
            _logger = logger;
        }
        public async Task LogRequest<TReq>(Guid correlationId, string serviceName, TReq request) where TReq : new()
        {
            var data = await _logObjectSerializer.Serialize(request);
            _logger.LogInformation("Request: {serviceName}:{correlationId} {data}", serviceName, correlationId, data);
        }

        public async Task LogResponse<TRes>(Guid correlationId, string serviceName, TRes response)
        {
            var data = await _logObjectSerializer.Serialize(response);
            _logger.LogInformation("Response: {serviceName}:{correlationId} {data}", serviceName, correlationId, data);
        }
    }
}