using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gtt.CodeWorks
{
    public interface IServiceInstance<in TRequest, TResponse> : IServiceInstance
        where TRequest : BaseRequest, new() where TResponse : new()
    {
        Task<ServiceResponse<TResponse>> Execute(TRequest request, DateTimeOffset startTime, CancellationToken cancellationToken);
    }

    public interface IServiceInstance
    {
        string Name { get; }
        DateTimeOffset StartTime { get; }
        Guid CorrelationId { get; }
        public ServiceAction Action { get; }
        Task<ServiceResponse> Execute(BaseRequest request, DateTimeOffset startTime, CancellationToken cancellationToken);
        Type RequestType { get; }
        Type ResponseType { get; }
    }
}
