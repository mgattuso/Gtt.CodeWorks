using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gtt.CodeWorks
{
    public interface IDistributedLockService
    {
        Task<DistributedLockStatus> CreateLock(string key, CancellationToken cancellationToken);
        Task ReleaseLock(string key, CancellationToken cancellationToken);
    }

    public enum DistributedLockStatus
    {
        NotLocked,
        Locked
    }
}
