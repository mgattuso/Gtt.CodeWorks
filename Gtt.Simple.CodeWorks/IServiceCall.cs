using System;
using System.Threading;
using System.Threading.Tasks;

namespace Gtt.Simple.CodeWorks
{
    public interface IServiceCall<in TRequest, TResponse>
        where TRequest : BaseRequest, new()
        where TResponse : class, new()
    {
        string Name { get; }
        string FullName { get; }
        Task<ServiceResponse<TResponse>> Execute(TRequest request, CancellationToken cancellationToken);
    }
}
