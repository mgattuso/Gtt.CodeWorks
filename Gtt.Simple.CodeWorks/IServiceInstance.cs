using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Gtt.Simple.CodeWorks
{
    public interface IServiceInstance<in TRequest, TResponse> : IServiceInstance, IServiceCall<TRequest, TResponse>
            where TRequest : BaseRequest, new() where TResponse : class, new()
    {
    }

    public interface IServiceInstance
    {
        string Name { get; }
        string FullName { get; }
        Task<ServiceResponse> Execute(BaseRequest request, CancellationToken cancellationToken);
        Type RequestType { get; }
        Type ResponseType { get; }
        IEnumerable<KeyValuePair<int, string>> AllErrorCodes();
        UserInformation? User { get; set; }
        CodeWorksEnvironment CurrentEnvironment { get; }
    }
}
