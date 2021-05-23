using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyNetQ;
using Microsoft.Extensions.Logging;

namespace Gtt.CodeWorks.EasyNetQ
{
    public class EasyNetQServiceLogger : IServiceLogger
    {
        private readonly ILogObjectSerializer _logObjectSerializer;
        private readonly IBus _bus;

        public EasyNetQServiceLogger(ILogObjectSerializer logObjectSerializer, IBus bus)
        {
            _logObjectSerializer = logObjectSerializer;
            _bus = bus;
        }
        public async Task LogRequest<TReq>(Guid correlationId, string serviceName, TReq request) where TReq : new()
        {
            var msg = await _logObjectSerializer.Serialize(request);
            await _bus.PubSub.PublishAsync(new ServiceLogMessage
            {
                CorrelationId = correlationId,
                Event = "Request",
                Service = serviceName,
                Message = msg
            });
        }

        public async Task LogResponse<TRes>(Guid correlationId, string serviceName, TRes response)
        {
            var msg = await _logObjectSerializer.Serialize(response);
            await _bus.PubSub.PublishAsync(new ServiceLogMessage
            {
                CorrelationId = correlationId,
                Event = "Response",
                Service = serviceName,
                Message = msg
            });
        }

        public LogLevel CurrentLogLevel => LogLevel.Information;
    }
}
