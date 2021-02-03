using System;

namespace Gtt.CodeWorks
{
    public interface IAuditable
    {
        DateTimeOffset Created { get; }
        string CreatedBy { get; }
        DateTimeOffset Modified { get; }
        string ModifiedBy { get; }
    }
}