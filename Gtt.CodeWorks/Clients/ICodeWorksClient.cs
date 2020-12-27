using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gtt.CodeWorks.Clients
{
    public interface ICodeWorksClient
    {
        public string ClientId { get; }
        public string[] RegisteredServices();
        Task<ServiceResponse<TResponse>> Call<TRequest, TResponse>(TRequest request, DateTimeOffset startTime, CancellationToken cancellationToken) 
            where TRequest : BaseRequest, TResponse
            where TResponse : new(); 
    }

    public interface IRealTimeClient : ICodeWorksClient
    {
    }

    public interface IDeferredClient : ICodeWorksClient
    {
    }

    public interface IHttpClient : IRealTimeClient
    {
        Uri BaseUri { get; }
        Task Ping();
        Task<HealthCheckResponse> HealthCheck();
    }

    public interface ILocalClient : IRealTimeClient
    {
    }
}
