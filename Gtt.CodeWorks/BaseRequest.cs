using System;
using System.Collections.Generic;
using System.Text;

namespace Gtt.CodeWorks
{
    public abstract class BaseRequest : ITraceable
    {
        public Guid CorrelationId { get; set; } = Guid.NewGuid();
        public Guid? SessionId { get; set; }
    }
}
