using System;
using System.Collections.Generic;
using System.Text;

namespace Gtt.CodeWorks
{
    public class ResponseMetaData
    {
        public ResponseMetaData(ServiceResult result, Guid correlationId, int durationMs)
        {
            Result = result;
            CorrelationId = correlationId;
            DurationMs = durationMs;
            ResponseCreated = DateTimeOffset.UtcNow;
        }

        public Guid CorrelationId { get; }
        public ServiceResult Result { get; }
        public int DurationMs { get; }
        public ResultCategory Category => Result.Category();
        public ResultOutcome Outcome => Result.Outcome();
        public DateTimeOffset ResponseCreated { get; }
    }
}
