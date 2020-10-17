using System.Threading;
using System.Threading.Tasks;

namespace Gtt.CodeWorks
{
    public class NullDistributedLockService : IDistributedLockService
    {
        private NullDistributedLockService()
        {
            
        }
        public Task<DistributedLockStatus> CreateLock(string key, CancellationToken cancellationToken)
        {
            return Task.FromResult(DistributedLockStatus.NotLocked);
        }

        public Task ReleaseLock(string key, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public static IDistributedLockService NoLock => new NullDistributedLockService();
    }
}