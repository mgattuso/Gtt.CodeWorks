using System;
using System.Collections.Generic;

namespace Gtt.Simple.CodeWorks
{
    public abstract class BaseRequest : ITraceable
    {
        public Guid CorrelationId { get; set; }
        public Guid? SessionId { get; set; }
        public string AuthToken { get; set; }
        public IDictionary<string, string[]> AdhocData { get; set; }
    }
}
