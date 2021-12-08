using System;

namespace Gtt.Simple.CodeWorks
{
    public interface ITraceable
    {
        Guid CorrelationId { get; set; }
    }
}
