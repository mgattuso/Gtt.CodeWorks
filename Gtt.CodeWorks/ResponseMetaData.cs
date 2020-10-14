using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Gtt.CodeWorks
{
    public class ResponseMetaData
    {
        private readonly DateTimeOffset _startTime;

        public ResponseMetaData(ServiceResult result, Guid correlationId, DateTimeOffset startTime)
        {
            _startTime = startTime;
            Result = result;
            CorrelationId = correlationId;
            ResponseCreated = DateTimeOffset.UtcNow;
        }

        public Guid CorrelationId { get; }
        public ServiceResult Result { get; }

        public long DurationMs => (long)(ServiceClock.CurrentTime() - _startTime).TotalMilliseconds;

        public DateTimeOffset ResponseCreated { get; }
    }
}
