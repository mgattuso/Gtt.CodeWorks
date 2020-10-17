using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Gtt.CodeWorks
{
    public class InMemoryDistributedLock : IDistributedLockService
    {
        private static readonly HashSet<string> HashSet = new HashSet<string>();
        private readonly object _lock = new object();

        public async Task<DistributedLockStatus> CreateLock(string key, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            if (string.IsNullOrWhiteSpace(key))
            {
                return DistributedLockStatus.NotLocked;
            }

            bool locked;

            lock (_lock)
            {
                locked = HashSet.Contains(key);
                if (!locked)
                {
                    HashSet.Add(key);
                }
            }

            return locked ? DistributedLockStatus.Locked : DistributedLockStatus.NotLocked;
        }

        public Task ReleaseLock(string key, CancellationToken cancellationToken)
        {
            lock (_lock)
            {
                HashSet.Remove(key);
            }

            return Task.CompletedTask;
        }
    }
}