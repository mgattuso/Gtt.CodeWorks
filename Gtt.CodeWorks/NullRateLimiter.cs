using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Gtt.CodeWorks
{
    public class NullRateLimiter : IRateLimiter
    {
        private NullRateLimiter()
        {

        }
        public Task<RateLimitStatus> EnterService(BaseRequest request)
        {
            return Task.FromResult(RateLimitStatus.NotLimited);
        }

        public Task LeaveService(BaseRequest request)
        {
            return Task.CompletedTask;
        }

        public static IRateLimiter NoLimits => new NullRateLimiter();
    }
}
