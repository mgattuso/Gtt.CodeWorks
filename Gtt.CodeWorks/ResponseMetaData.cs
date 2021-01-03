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
        private readonly DateTimeOffset _startTime;

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
            _startTime = service.StartTime;
            Result = result;
            CorrelationId = service.CorrelationId;
            Dependencies = dependencies;
            ResponseCreated = ServiceClock.CurrentTime();
            ServiceName = service.FullName;
        }

        public ResponseMetaData(IServiceInstance service, ValidationErrorResponse validationErrors, Dictionary<string, ResponseMetaData> dependencies = null)
        {
            _startTime = service.StartTime;
            Result = ServiceResult.ValidationError;
            CorrelationId = service.CorrelationId;
            Dependencies = dependencies;
            ResponseCreated = ServiceClock.CurrentTime();
            ServiceName = service.FullName;
            Errors = validationErrors.ToDictionary();
        }

        public ResponseMetaData(
            IServiceInstance service,
            ServiceResult result,
            ErrorData error,
            Dictionary<string, ResponseMetaData> dependencies = null)
        {
            _startTime = service.StartTime;
            Result = result;
            CorrelationId = service.CorrelationId;
            Dependencies = dependencies;
            ResponseCreated = ServiceClock.CurrentTime();
            ServiceName = service.FullName;
            Errors = error.ToDictionary();
        }

        public ResponseMetaData(
            string serviceName,
            DateTimeOffset responseCreated,
            long durationMs,
            Guid correlationId,
            ServiceResult result,
            IDictionary<string, string[]> errors,
            Dictionary<string, ResponseMetaData> dependencies = null)
        {
            Result = result;
            CorrelationId = correlationId;
            Dependencies = dependencies;
            ResponseCreated = responseCreated;
            ServiceName = serviceName;
            Errors = errors != null ? new Dictionary<string, string[]>(errors) : new Dictionary<string, string[]>();
            DurationMs = durationMs;
        }

        public string ServiceName { get; }
        public Guid CorrelationId { get; set; }
        public ServiceResult Result { get; }
        public long DurationMs { get; }
        public DateTimeOffset ResponseCreated { get; }
        public Dictionary<string, string[]> Errors { get; }
        public Dictionary<string, ResponseMetaData> Dependencies { get; }
    }
}
