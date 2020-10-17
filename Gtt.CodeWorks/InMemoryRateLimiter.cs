using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Gtt.CodeWorks
{
    public class InMemoryRateLimiter : IRateLimiter
    {
        private readonly int _maxConcurrentAttemptsHardLimit;
        private readonly int _maxConcurrentAttemptsSoftLimit;
        private static readonly ConcurrentDictionary<Guid, int> Dict = new ConcurrentDictionary<Guid, int>();

        public InMemoryRateLimiter(
            int maxConcurrentAttemptsHardLimit = 4,
            int maxConcurrentAttemptsSoftLimit = 2)
        {
            _maxConcurrentAttemptsHardLimit = maxConcurrentAttemptsHardLimit;
            _maxConcurrentAttemptsSoftLimit = maxConcurrentAttemptsSoftLimit;
        }

        public async Task<RateLimitStatus> EnterService(BaseRequest request)
        {
            await Task.CompletedTask;
            if (request.SessionId == null)
            {
                return RateLimitStatus.NotLimited;
            }

            var key = request.SessionId.Value;

            var count = Dict.AddOrUpdate(key, guid => 1, (guid, i) => ++i);

            if (count > _maxConcurrentAttemptsHardLimit)
            {
                return RateLimitStatus.HardLimit;
            }

            if (count > _maxConcurrentAttemptsSoftLimit)
            {
                return RateLimitStatus.SoftLimit;
            }

            return RateLimitStatus.NotLimited;
        }

        public async Task LeaveService(BaseRequest request)
        {
            await Task.CompletedTask;
            if (request.SessionId == null)
            {
                return;
            }

            var key = request.SessionId.Value;
            Dict.AddOrUpdate(key, guid => 0, (guid, i) => --i);
        }
    }
}