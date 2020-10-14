using System;
using System.Collections.Generic;
using System.Text;

namespace Gtt.CodeWorks
{
    public abstract class BaseRequest
    {
        public Guid CorrelationId { get; set; }
    }
}
