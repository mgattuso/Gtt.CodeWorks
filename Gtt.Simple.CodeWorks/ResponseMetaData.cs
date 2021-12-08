using System;
using System.Collections.Generic;
using System.Linq;

namespace Gtt.Simple.CodeWorks
{
    public class ResponseMetaData : ITraceable
    {
#if !DEBUG
        /// <summary>
        /// Used for Deserialization
        /// </summary>
        public ResponseMetaData()
        {

        }
#endif
        public ResponseMetaData(
            IServiceInstance service,
            ServiceResult result,
            TimeSpan duration,
            Guid correlationId,
            IDictionary<int, string>? errorCodes = null,
            IDictionary<string, string[]>? validationErrors = null,
            string? publicMessage = "",
            string? exceptionMessage = "",
            Dictionary<string, ResponseMetaData>? dependencies = null
            ) : this(
            service.FullName,
            correlationId,
            result,
            (long)duration.TotalMilliseconds,
            responseCreated: null,
            errorCodes: errorCodes,
            validationErrors: validationErrors,
            publicMessage: publicMessage,
            exceptionMessage: exceptionMessage,
            dependencies: dependencies)
        {

        }

        public ResponseMetaData(
            string serviceName,
            Guid correlationId,
            ServiceResult serviceResult,
            long durationMs,
            DateTimeOffset? responseCreated = null,
            IDictionary<int, string>? errorCodes = null,
            IDictionary<string, string[]>? validationErrors = null,
            string? publicMessage = "",
            string? exceptionMessage = "",
            Dictionary<string, ResponseMetaData>? dependencies = null
        )
        {
            ServiceName = serviceName;
            CorrelationId = correlationId;
            Result = serviceResult;
            DurationMs = durationMs;
            PublicMessage = string.IsNullOrWhiteSpace(publicMessage) ? null : publicMessage.Trim();
            ExceptionMessage = string.IsNullOrWhiteSpace(exceptionMessage) ? null : exceptionMessage.Trim();
            Dependencies = dependencies;
            ErrorCodes = errorCodes?.ToDictionary(k => k.Key.ToString(), v => v.Value);
            ValidationErrors = validationErrors;
            ResponseCreated = responseCreated ?? ServiceClock.CurrentTime();
        }

        [AlwaysPresent]
        public string ServiceName { get; }
        [AlwaysPresent]
        public Guid CorrelationId { get; set; }

        [AlwaysPresent]
        public ServiceResult Result { get; }
        [AlwaysPresent]
        public long DurationMs { get; internal set; }
        [AlwaysPresent]
        public DateTimeOffset ResponseCreated { get; }
        public IDictionary<string, string>? ErrorCodes { get; }
        public IDictionary<string, string[]>? ValidationErrors { get; }
        public string? PublicMessage { get; }
        public string? ExceptionMessage { get; set; }
        public IDictionary<string, ResponseMetaData>? Dependencies { get; }
    }
}
