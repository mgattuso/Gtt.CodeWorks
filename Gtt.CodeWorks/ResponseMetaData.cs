using System;
using System.Collections.Generic;
using System.Text;

namespace Gtt.CodeWorks
{
    public class ResponseMetaData
    {
        public ResponseMetaData(ServiceResult result)
        {
            Result = result;
            ResponseCreated = DateTimeOffset.UtcNow;
        }

        public ServiceResult Result { get; }
        public ResultCategory Category => Result.Category();
        public ResultOutcome Outcome => Result.Outcome();
        public DateTimeOffset ResponseCreated { get; }
    }
}
