using System;

namespace Gtt.CodeWorks
{
    public abstract class BaseDto : ITraceable
    {
        public Guid CorrelationId { get; set; }

    }
}