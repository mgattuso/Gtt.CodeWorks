using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Gtt.CodeWorks
{
    public interface IRateLimiter
    {
        Task<RateLimitStatus> EnterService(BaseRequest request);
        Task LeaveService(BaseRequest request);
    }

    public enum RateLimitStatus
    {
        NotLimited,
        SoftLimit,
        HardLimit
    }
}
