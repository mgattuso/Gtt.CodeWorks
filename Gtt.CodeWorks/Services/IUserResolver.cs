using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gtt.CodeWorks.Services
{
    public interface IUserResolver
    {
        Task<UserResolverResult> GetUserOrDefault(string authToken, Guid correlationId, CancellationToken cancellationToken);
    }
}
