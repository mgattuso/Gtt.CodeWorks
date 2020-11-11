using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

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
            ServiceName = service.Name;
        }

        public ResponseMetaData(
            IServiceInstance service,
            ServiceResult result,
            ErrorData error)
        {
            _startTime = service.StartTime;
            Result = result;
            CorrelationId = service.CorrelationId;
            Errors = error.ToDictionary();
            ResponseCreated = ServiceClock.CurrentTime();
            ServiceName = service.Name;
        }

        public string ServiceName { get; }
        public Guid CorrelationId { get; set; }
        public ServiceResult Result { get; }
        public long DurationMs => (long)(ServiceClock.CurrentTime() - _startTime).TotalMilliseconds;
        public DateTimeOffset ResponseCreated { get; }
        public Dictionary<string, string[]> Errors { get; }
        public Dictionary<string, ResponseMetaData> Dependencies { get; }
    }
}
