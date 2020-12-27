using System;
using System.Threading;
using System.Threading.Tasks;

namespace Gtt.CodeWorks
{
    public interface IServiceCall<in TRequest, TResponse> 
        where TRequest : BaseRequest, new()
        where TResponse : new()
    {
        Task<ServiceResponse<TResponse>> Execute(TRequest request, DateTimeOffset startTime, CancellationToken cancellationToken);
    }
}