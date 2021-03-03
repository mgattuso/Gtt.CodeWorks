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

    public interface ILocalServiceInstance : IServiceInstance
    {
        ServiceResponse StoredResponse { get; }
    }
    public interface ILocalServiceInstance<in TRequest, TResponse> : IServiceInstance
        where TRequest : BaseRequest, new() where TResponse : new()
    {

        Task<ServiceResponse<TResponse>> ExecuteLazyLocal(TRequest request,
            CancellationToken cancellationToken);

        Task<ServiceResponse<TResponse>> ExecuteLocal(TRequest request,
            CancellationToken cancellationToken);
    }

    public interface IServiceInstance
    {
        string Name { get; }
        string FullName { get; }
        DateTimeOffset StartTime { get; }
        Guid CorrelationId { get; }
        Guid? SessionId { get; }
        int? ServiceHop { get; }
        ServiceAction Action { get; }
        Task<ServiceResponse> Execute(BaseRequest request, DateTimeOffset startTime, CancellationToken cancellationToken);
        Type RequestType { get; }
        Type ResponseType { get; }
        IEnumerable<KeyValuePair<int, string>> AllErrorCodes();
        UserInformation User { get; set; }
        CodeWorksEnvironment CurrentEnvironment { get; }
    }

    public interface IAuthenticatedServiceInstance
    {
        IAccessPolicy AccessPolicy { get; }
    }
}
