using System;
using System.Threading;
using System.Threading.Tasks;

namespace Gtt.Simple.CodeWorks
{
    public interface ILocalServiceInstance : IServiceInstance
    {
        BaseRequest StoredRequest { get; }
        ServiceResponse StoredResponse { get; }
    }

    public interface ILocalServiceInstance<TRequest, TResponse> : IServiceInstance
    where TRequest : BaseRequest, new() where TResponse : class, new()
    {

        Task<ServiceResponse<TResponse>> ExecuteTakeCached(TRequest request, CancellationToken cancellationToken);

        Task<ServiceResponse<TResponse>> ExecuteTakeCached(TRequest request, Func<TRequest, bool> predicate,
            CancellationToken cancellationToken);
    }
}
