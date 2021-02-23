using System;
using System.Threading;
using System.Threading.Tasks;

namespace Gtt.CodeWorks.Services
{
    public interface IAuthTokenGenerator
    {
        Task<string> Generate(UserInformation user, Guid correlationId, TimeSpan? expiresIn, CancellationToken cancellationToken);
    }
}