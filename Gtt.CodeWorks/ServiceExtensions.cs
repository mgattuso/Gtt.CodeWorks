using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gtt.CodeWorks
{
    public static class ServiceExtensions
    {
        public static Task<ServiceResponse<TRes>> CallService<TService, TReq, TRes>(this IServiceInstance service, TRes request)
            where TService : IServiceInstance<TReq, TRes>
            where TRes : new()
            where TReq : BaseRequest, new()
        {
            return null;
        }
    }
}
