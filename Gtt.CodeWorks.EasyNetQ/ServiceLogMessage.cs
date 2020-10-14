using System;
using System.Collections.Generic;
using System.Text;

namespace Gtt.CodeWorks.EasyNetQ
{
    public class ServiceLogMessage
    {
        public Guid CorrelationId { get; set; }
        public string Service { get; set; }
        public string Event { get; set; }
        public string Message { get; set; }
    }
}
