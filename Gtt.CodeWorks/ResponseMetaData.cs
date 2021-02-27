using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Gtt.CodeWorks.Validation;

namespace Gtt.CodeWorks
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
            Dictionary<string, ResponseMetaData> dependencies = null)
        {
            var startTime = service.StartTime;
            Result = result;
            CorrelationId = service.CorrelationId;
            Dependencies = dependencies;
            ResponseCreated = ServiceClock.CurrentTime();
            DurationMs = (long)(ResponseCreated-startTime).TotalMilliseconds;
            ServiceName = service.FullName;
        }

        public ResponseMetaData(IServiceInstance service, ValidationErrorResponse validationErrors, Dictionary<string, ResponseMetaData> dependencies = null)
        {
            var startTime = service.StartTime;
            Result = ServiceResult.ValidationError;
            CorrelationId = service.CorrelationId;
            Dependencies = dependencies;
            ResponseCreated = ServiceClock.CurrentTime();
            DurationMs = (long)(ResponseCreated - startTime).TotalMilliseconds;
            ServiceName = service.FullName;
            Errors = validationErrors.ToDictionary();
        }

        public ResponseMetaData(
            IServiceInstance service,
            ServiceResult result,
            ErrorData error,
            Dictionary<string, ResponseMetaData> dependencies = null)
        {
            var startTime = service.StartTime;
            Result = result;
            CorrelationId = service.CorrelationId;
            Dependencies = dependencies;
            ResponseCreated = ServiceClock.CurrentTime();
            DurationMs = (long)(ResponseCreated - startTime).TotalMilliseconds;
            ServiceName = service.FullName;
            Errors = error?.ToDictionary();
        }

        public ResponseMetaData(
            string serviceName,
            DateTimeOffset responseCreated,
            long durationMs,
            Guid correlationId,
            ServiceResult result,
            IDictionary<string, object> errors,
            Dictionary<string, ResponseMetaData> dependencies = null)
        {
            Result = result;
            CorrelationId = correlationId;
            Dependencies = dependencies;
            ResponseCreated = responseCreated;
            ServiceName = serviceName;
            Errors = errors != null ? new Dictionary<string, object>(errors) : new Dictionary<string, object>();
            DurationMs = durationMs;
        }

        [AlwaysPresent]
        public string ServiceName { get; }
        [AlwaysPresent]
        public Guid CorrelationId { get; set; }
        [AlwaysPresent]
        public ServiceResult Result { get; }
        [AlwaysPresent]
        public long DurationMs { get; }
        [AlwaysPresent]
        public DateTimeOffset ResponseCreated { get; }
        public Dictionary<string, object> Errors { get; }
        public Dictionary<string, ResponseMetaData> Dependencies { get; }
    }
}
