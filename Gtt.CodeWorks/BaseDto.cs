using System;

namespace Gtt.CodeWorks
{
    public abstract class BaseDto : ITraceable, IAuditable
    {
        public Guid CorrelationId { get; set; }
        public DateTimeOffset Created { get; set; } = ServiceClock.CurrentTime();
        public string CreatedBy { get; set; }
        public DateTimeOffset Modified { get; set; } = ServiceClock.CurrentTime();
        public string ModifiedBy { get; set; }
    }
}