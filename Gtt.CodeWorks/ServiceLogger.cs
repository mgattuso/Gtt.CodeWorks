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

        private static LogLevel? _currentLogLevel;

        public LogLevel CurrentLogLevel
        {
            get
            {
                if (_currentLogLevel != null) 
                    return _currentLogLevel.Value;

                Array levels = Enum.GetValues(typeof(LogLevel));
                for (int i = 0; i < levels.Length; i++)
                {
                    var l = levels.GetValue(i);
                    if (_logger.IsEnabled((LogLevel) l))
                    {
                        _currentLogLevel = (LogLevel) l;
                        break;
                    }
                }

                if (_currentLogLevel == null)
                    _currentLogLevel = LogLevel.None;

                return _currentLogLevel.Value;
            }
        }
    }
}