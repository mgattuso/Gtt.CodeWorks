using System.Threading;
using System.Threading.Tasks;

namespace Gtt.CodeWorks.Middleware
{
    public interface IServiceMiddleware
    {
        Task<ServiceResponse> OnRequest<TReq>(
            IServiceInstance service, 
            TReq request, 
            CancellationToken cancellationToken)
            where TReq : BaseRequest, new();

        Task OnResponse<TReq, TRes>(
                IServiceInstance service, 
                TReq request, 
                ServiceResponse<TRes> response,
                CancellationToken cancellationToken)
            where TReq : BaseRequest, new()
            where TRes : new();

        bool IgnoreExceptions { get; }
        bool SkipOnInternalCall { get; }
    }

    public static class ServiceMiddlewareExtensions
    {
        public static ServiceResponse ContinuePipeline(this IServiceMiddleware middleware)
        {
            return null;
        }
    }
}