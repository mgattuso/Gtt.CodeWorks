using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Gtt.CodeWorks
{
    public class ResponseMetaData : ITraceable
    {
        private readonly DateTimeOffset _startTime;

        public ResponseMetaData(ServiceResult result, Guid correlationId, DateTimeOffset startTime)
        {
            _startTime = startTime;
            Result = result;
            CorrelationId = correlationId;
            ResponseCreated = DateTimeOffset.UtcNow;
        }

        public ResponseMetaData(
            ServiceResult result, 
            Guid correlationId, 
            DateTimeOffset startTime,
            string errorInformation)
        {
            _startTime = startTime;
            Result = result;
            CorrelationId = correlationId;
            ErrorInformation = errorInformation;
            ResponseCreated = DateTimeOffset.UtcNow;
        }

        public Guid CorrelationId { get; set; }
        public ServiceResult Result { get; }
        public long DurationMs => (long)(ServiceClock.CurrentTime() - _startTime).TotalMilliseconds;
        public DateTimeOffset ResponseCreated { get; }

        public string ErrorInformation { get; }
    }
}
