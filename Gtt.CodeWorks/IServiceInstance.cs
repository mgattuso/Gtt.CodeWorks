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
        BaseRequest StoredRequest { get; }
        ServiceResponse StoredResponse { get; }
    }
    public interface ILocalServiceInstance<TRequest, TResponse> : IServiceInstance
        where TRequest : BaseRequest, new() where TResponse : new()
    {

        Task<ServiceResponse<TResponse>> ExecuteTakeCached(TRequest request,
            CancellationToken cancellationToken);

        Task<ServiceResponse<TResponse>> ExecuteTakeCached(TRequest request, Func<TRequest, bool> predicate,
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
        Task<ServiceResponse> Execute(BaseRequest request, CancellationToken cancellationToken);
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
