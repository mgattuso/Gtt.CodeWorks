using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gtt.CodeWorks
{
    public interface IServiceInstance<in TRequest, TResponse> : IServiceInstance, IServiceCall<TRequest, TResponse>
        where TRequest : BaseRequest, new() where TResponse : new()
    {
    }

    public interface IServiceInstance
    {
        string Name { get; }
        string FullName { get; }
        DateTimeOffset StartTime { get; }
        Guid CorrelationId { get; }
        Guid? SessionId { get; }
        int? ServiceHop { get; }
        public ServiceAction Action { get; }
        Task<ServiceResponse> Execute(BaseRequest request, DateTimeOffset startTime, CancellationToken cancellationToken);
        Type RequestType { get; }
        Type ResponseType { get; }
    }
}
